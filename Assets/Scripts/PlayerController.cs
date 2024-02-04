using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviour, IPunObservable
{
    public Rigidbody2D rb;
    public Transform spriteTransform;
    [SerializeField] Transform playerCenter;
    public Collider2D playerCollider;
    [SerializeField] LayerMask groundLayerMask;

    [SerializeField] float airResistance = 0;

    [SerializeField] PlayerInfoUI playerInfoUI;
    public ControllerUI controllerUI;

    public Player photonPlayer;
    public int playerNum;

    public SpriteRenderer playerSr;
    public Animator animator;

    [SerializeField] Transform landingSmokeSpawn;

    bool forceBurst = false;

    public bool isGrounded = true;
    public bool facingLeft = false;
    public bool toFlip = false;

    List<PlayerActions> unavailableActions = new();

    public enum PlayerActions
    {
        //Movement
        WAIT,
        WALK_LEFT, //walks forward instead
        WALK_RIGHT, //not in use
        ROLL,
        JUMP,
        FALL,

        //Defense
        BLOCK,
        BURST,
        SKIP,

        //Attack
        HYDRO_BALL, //not in use
        ICICLE,
        LINGERING_SPIRIT,
        EXPLOSION,
        LIGHTNING,
        SEISMIC_STRIKE,
        WHIRLWIND,

        //Others
        STUNNED,
        CONTINUE_BLOCK,
        STOP_BLOCK,
        NONE,
        TELEPORT
    }

    public PlayerActions playerCurrentAction = PlayerActions.NONE;
    public int currentFrameNum = 0;

    //Player stats
    public float burstMeterValue = 0;
    public int knockbackMultiplier = 0;
    public int airOptionsAvail = 4;
    public const int airOptionsMax = 4;

    public bool allowMove = false;

    //Actions
    FrameBehaviour currentFrameBehaviour;
    PlayerWait playerWait;
    PlayerWalk playerWalk;
    PlayerRoll playerRoll;
    PlayerJump playerJump;
    PlayerFall playerFall;
    PlayerTeleport playerTeleport;
    PlayerStun playerStun;

    PlayerIcicle playerIcicle;
    PlayerExplosion playerExplosion;
    PlayerLingeringSpirit playerLingeringSpirit;
    PlayerLightning playerLightning;
    PlayerSeismicStrike playerSeismicStrike;
    PlayerWhirlwind playerWhirlwind;

    PlayerBlock playerBlock;
    PlayerCancelBlock playerCancelBlock;
    PlayerBurst playerBurst;

    //Store attacks for their cooldowns
    Dictionary<PlayerActions, PlayerFrameBehaviour> playerAttacks = new();

    //Opponent stuff
    public Transform oppTransform;

    //Replay stuff
    public bool isDead = false;
    public string playerName;
    public int playerSkinID;

    #region IPunObservable implementation

    //For syncing data via IPunObservable
    const byte BURST_FLAG = 1 << 0;
    const byte KNOCKBACK_FLAG = 1 << 1;
    const byte AIR_FLAG = 1 << 2;

    float syncedBurstMeterValue = 0;
    int syncedKnockbackMultiplier = 0;
    int syncedairOptionsAvail = 0;

    byte syncDataFlags;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            syncDataFlags = 0;

            //Check which variables have changed
            if (syncedBurstMeterValue != burstMeterValue)
            {
                syncDataFlags |= BURST_FLAG;
            }
            if (syncedKnockbackMultiplier != knockbackMultiplier)
            {
                syncDataFlags |= KNOCKBACK_FLAG;
            }
            if (syncedairOptionsAvail != airOptionsAvail)
            {
                syncDataFlags |= AIR_FLAG;
            }

            //Send data flags
            stream.SendNext(syncDataFlags);

            //Send variables that changed
            if ((syncDataFlags & BURST_FLAG) != 0)
            {
                stream.SendNext(burstMeterValue);
                syncedBurstMeterValue = burstMeterValue;
            }
            if ((syncDataFlags & KNOCKBACK_FLAG) != 0)
            {
                stream.SendNext(knockbackMultiplier);
                syncedKnockbackMultiplier = knockbackMultiplier;
            }
            if ((syncDataFlags & AIR_FLAG) != 0)
            {
                stream.SendNext(airOptionsAvail);
                syncedairOptionsAvail = airOptionsAvail;
            }
        }
        else
        {
            //Receive data flags
            syncDataFlags = (byte)stream.ReceiveNext();

            //Receive and update variables based on data flags
            if ((syncDataFlags & BURST_FLAG) != 0)
            {
                burstMeterValue = (float)stream.ReceiveNext();
            }
            if ((syncDataFlags & KNOCKBACK_FLAG) != 0)
            {
                knockbackMultiplier = (int)stream.ReceiveNext();
            }
            if ((syncDataFlags & AIR_FLAG) != 0)
            {
                airOptionsAvail = (int)stream.ReceiveNext();
            }
        }
    }

    #endregion

    private void Awake()
    {
        SetPlayerInfo();

        playerWait = GetComponent<PlayerWait>();
        playerWalk = GetComponent<PlayerWalk>();
        playerRoll = GetComponent<PlayerRoll>();
        playerJump = GetComponent<PlayerJump>();
        playerFall = GetComponent<PlayerFall>();
        playerTeleport = GetComponent<PlayerTeleport>();
        playerStun = GetComponent<PlayerStun>();

        playerIcicle = GetComponent<PlayerIcicle>();
        playerExplosion = GetComponent<PlayerExplosion>();
        playerLingeringSpirit = GetComponent<PlayerLingeringSpirit>();
        playerLightning = GetComponent<PlayerLightning>();
        playerSeismicStrike = GetComponent<PlayerSeismicStrike>();
        playerWhirlwind = GetComponent<PlayerWhirlwind>();

        playerBlock = GetComponent<PlayerBlock>();
        playerCancelBlock = GetComponent<PlayerCancelBlock>();
        playerBurst = GetComponent<PlayerBurst>();

        playerAttacks.Add(PlayerActions.ICICLE, playerIcicle);
        playerAttacks.Add(PlayerActions.EXPLOSION, playerExplosion);
        playerAttacks.Add(PlayerActions.LINGERING_SPIRIT, playerLingeringSpirit);
        playerAttacks.Add(PlayerActions.LIGHTNING, playerLightning);
        playerAttacks.Add(PlayerActions.SEISMIC_STRIKE, playerSeismicStrike);
        playerAttacks.Add(PlayerActions.WHIRLWIND, playerWhirlwind);
    }

    void SetPlayerInfo()
    {
        if (!ShooterGameManager.Instance.isReplay)
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_NUMBER, out object playerNumber))
                {
                    if ((int)playerNumber == playerNum)
                    {
                        photonPlayer = p;

                        playerName = p.NickName;

                        if (p.IsLocal)
                            playerInfoUI.SetPlayerName(playerName + " (YOU)");
                        else
                            playerInfoUI.SetPlayerName(playerName);

                        if (p.CustomProperties.TryGetValue(ShooterGameInfo.PLAYER_SKIN, out object _playerSkinID))
                        {
                            playerSkinID = (int)_playerSkinID;

                            playerSr.material.SetColor("_PlayerColor", ShooterGameInfo.GetColor(playerSkinID));

                            playerInfoUI.SetUISkin(playerSkinID);
                        }

                        return;
                    }

                }
            }
        }
        else
        {
            ReplayPlayer replayPlayer = null;
            switch (playerNum)
            {
                case 1:
                    replayPlayer = ReplayManager.Instance.replay.p1;
                    break;
                case 2:
                    replayPlayer = ReplayManager.Instance.replay.p2;
                    break;
            }

            if (replayPlayer != null)
            {
                playerName = replayPlayer.playerName;

                if (replayPlayer.isMe)
                    playerInfoUI.SetPlayerName(playerName + " (YOU)");
                else
                    playerInfoUI.SetPlayerName(playerName);

                playerSkinID = replayPlayer.skinID;

                playerSr.material.SetColor("_PlayerColor", ShooterGameInfo.GetColor(playerSkinID));

                playerInfoUI.SetUISkin(playerSkinID);
            }
            else
            {
                Debug.Log("Replay Player is null");
            }
        }
    }

    public void UpdateInfoUIManual(float playerBurstMeterVal, int playerKnockbackMulti, int playerAirOptions)
    {
        playerInfoUI.UpdatePlayerInfo(playerBurstMeterVal, playerKnockbackMulti, playerAirOptions);
    }

    public void UpdateInfoUIAuto()
    {
        playerInfoUI.UpdatePlayerInfo(burstMeterValue, knockbackMultiplier, airOptionsAvail);
    }

    public void CheckMoves()
    {
        unavailableActions.Clear();

        forceBurst = IsStunned() && CanBurst();

        if (!forceBurst)
        {
            bool blocking = playerCurrentAction == PlayerActions.BLOCK;
            if (!blocking)
            {
                if (!CanTp())
                {
                    unavailableActions.Add(PlayerActions.TELEPORT);
                }

                //Check movement
                if (!isGrounded)
                {
                    unavailableActions.Add(PlayerActions.WALK_LEFT);
                    unavailableActions.Add(PlayerActions.SEISMIC_STRIKE);

                    if (airOptionsAvail <= 0)
                    {
                        unavailableActions.Add(PlayerActions.ROLL);
                        unavailableActions.Add(PlayerActions.JUMP);
                        unavailableActions.Add(PlayerActions.FALL);
                    }
                }
                else
                {
                    unavailableActions.Add(PlayerActions.ROLL);
                    unavailableActions.Add(PlayerActions.FALL);
                }

                //Add skip and burst (only available for force burst)
                unavailableActions.Add(PlayerActions.BURST);
                unavailableActions.Add(PlayerActions.SKIP);

                //Check attacks (cant use attacks on cooldown)
                foreach (KeyValuePair<PlayerActions, PlayerFrameBehaviour> playerAttack in playerAttacks)
                {
                    if (playerAttack.Value.currentCooldown >= 0)
                    {
                        unavailableActions.Add(playerAttack.Key);
                    }
                }

                //Add continue and stop block
                unavailableActions.Add(PlayerActions.CONTINUE_BLOCK);
                unavailableActions.Add(PlayerActions.STOP_BLOCK);
            }
        }
    }

    public bool CanBurst()
    {
        return burstMeterValue >= 0.5;
    }

    public bool CanTp()
    {
        return burstMeterValue >= 0.5;
    }

    public bool IsIdle()
    {
        return playerCurrentAction == PlayerActions.NONE;
    }

    public bool IsInterruptable()
    {
        return (playerCurrentAction == PlayerActions.WAIT) ||
               (playerCurrentAction == PlayerActions.BLOCK);
    }

    public void ShowControls(bool active)
    {
        CheckMoves();

        if (active)
        {
            //Set ui first
            controllerUI.SetUI(unavailableActions, forceBurst, playerCurrentAction == PlayerActions.BLOCK);
        }

        controllerUI.gameObject.SetActive(active);
    }

    public void RunFrameBehaviour()
    {
        if (toFlip)
        {
            toFlip = false;
            FlipPlayer();
        }

        if (currentFrameNum == 0)
        {
            if (currentFrameBehaviour != null) //will only be null the first time
            {
                currentFrameBehaviour.EndAnimation();
                currentFrameBehaviour = null;
            }

            switch (playerCurrentAction)
            {
                //Movement

                case PlayerActions.WAIT:
                    currentFrameBehaviour = playerWait;
                    break;

                case PlayerActions.WALK_LEFT:
                    if (facingLeft)
                        playerWalk.goLeft = true;
                    else
                        playerWalk.goLeft = false;

                    currentFrameBehaviour = playerWalk;
                    break;

                case PlayerActions.ROLL:
                    if (facingLeft)
                        playerRoll.goLeft = true;
                    else
                        playerRoll.goLeft = false;

                    if (!isGrounded) airOptionsAvail -= 1;

                    currentFrameBehaviour = playerRoll;
                    break;

                case PlayerActions.JUMP:
                    if (facingLeft)
                        playerJump.goLeft = true;
                    else
                        playerJump.goLeft = false;

                    if (!isGrounded) airOptionsAvail -= 1;

                    currentFrameBehaviour = playerJump;
                    break;

                case PlayerActions.FALL:
                    playerFall.startFall = true;

                    if (!isGrounded) airOptionsAvail -= 1;

                    currentFrameBehaviour = playerFall;
                    break;

                case PlayerActions.TELEPORT:
                    if (facingLeft)
                        playerTeleport.goLeft = true;
                    else
                        playerTeleport.goLeft = false;

                    burstMeterValue -= 0.5f;
                    if (burstMeterValue < 0)
                    {
                        burstMeterValue = 0;
                    }

                    currentFrameBehaviour = playerTeleport;
                    break;

                //Defense

                case PlayerActions.BLOCK:
                    currentFrameBehaviour = playerBlock;
                    break;

                case PlayerActions.STOP_BLOCK:
                    currentFrameBehaviour = playerCancelBlock;
                    break;

                case PlayerActions.BURST:
                    burstMeterValue -= 0.5f;
                    if (burstMeterValue < 0)
                    {
                        burstMeterValue = 0;
                    }

                    currentFrameBehaviour = playerBurst;
                    break;

                //Attack

                case PlayerActions.ICICLE:
                    if (facingLeft) playerIcicle.goLeft = true;
                    else playerIcicle.goLeft = false;

                    currentFrameBehaviour = playerIcicle;
                    break;

                case PlayerActions.LINGERING_SPIRIT:
                    if (facingLeft) playerLingeringSpirit.goLeft = true;
                    else playerLingeringSpirit.goLeft = false;

                    currentFrameBehaviour = playerLingeringSpirit;
                    break;

                case PlayerActions.EXPLOSION:
                    if (facingLeft) playerExplosion.goLeft = true;
                    else playerExplosion.goLeft = false;

                    currentFrameBehaviour = playerExplosion;
                    break;

                case PlayerActions.LIGHTNING:
                    currentFrameBehaviour = playerLightning;
                    break;

                case PlayerActions.SEISMIC_STRIKE:
                    if (facingLeft) playerSeismicStrike.goLeft = true;
                    else playerSeismicStrike.goLeft = false;

                    currentFrameBehaviour = playerSeismicStrike;
                    break;

                case PlayerActions.WHIRLWIND:
                    if (facingLeft) playerWhirlwind.goLeft = true;
                    else playerWhirlwind.goLeft = false;
                    currentFrameBehaviour = playerWhirlwind;
                    break;

                case PlayerActions.STUNNED:
                    currentFrameBehaviour = playerStun;
                    break;
            }

            if (currentFrameBehaviour != null)
            {
                if (playerCurrentAction != PlayerActions.SKIP)
                {
                    currentFrameBehaviour.enabledBehaviour = true;
                    currentFrameBehaviour.lastFrame = false;
                    currentFrameBehaviour.currentCooldown = currentFrameBehaviour.maxCooldown;
                }
            }
            else
            {
                Debug.Log("No new behaviour selected");
            }
        }

        if (currentFrameBehaviour != null)
        {
            currentFrameBehaviour.frameNum = currentFrameNum;
            currentFrameBehaviour.GoToFrame();

            if (currentFrameBehaviour.IsAnimationDone())
            {
                playerCurrentAction = PlayerActions.NONE;
            }
        }

        foreach (KeyValuePair<PlayerActions, PlayerFrameBehaviour> playerAttack in playerAttacks)
        {
            --playerAttack.Value.currentCooldown;
        }

        //update spellBlock
        if (playerBlock.spellBlock.activeSpell)
        {
            playerBlock.spellBlock.GoToFrame();
            ++playerBlock.spellBlock.frameNum;
        }
    }

    void ApplyAirResistance()
    {
        Vector2 resistiveForce = -rb.velocity * airResistance;

        rb.AddForce(resistiveForce, ForceMode2D.Impulse);
    }

    public void ApplyResistances()
    {
        if (!isGrounded) ApplyAirResistance();
    }

    public void FlipPlayer()
    {
        spriteTransform.localScale = new Vector3(spriteTransform.localScale.x * -1, 1, 1);

        facingLeft = !facingLeft;
    }

    public bool CheckIfGrounded()
    {
        bool wasGrounded = isGrounded;

        isGrounded = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0, Vector2.down, 0.025f, groundLayerMask);

        if (!wasGrounded && isGrounded)
        {
            GameObject particleObj = ShooterGameManager.Instance.GetPooledSpell("LandingSmoke");

            SpellFrameBehaviour spellParticle = particleObj.GetComponent<SpellFrameBehaviour>();
            spellParticle.spawnPos = landingSmokeSpawn.position;
        }

        return isGrounded;
    }

    public void RefillAirOptions()
    {
        if (isGrounded)
        {
            if (airOptionsAvail < airOptionsMax) airOptionsAvail = airOptionsMax;
        }
    }

    public void ForceSetGrounded(bool grounded)
    {
        isGrounded = grounded;
    }

    public bool IsStunned()
    {
        return playerCurrentAction == PlayerActions.STUNNED;
    }

    public void TakeHit(int knockbackIncrease, Vector2 knockbackForce, int stunDuration, bool unblockable = false, bool ignoreMultiplier = false)
    {
        if (playerCurrentAction != PlayerActions.BLOCK || unblockable)
        {
            if (playerCurrentAction == PlayerActions.BLOCK)
            {
                playerBlock.ShrinkSpellBlock();
            }

            float finalKnockbackMultiplier = 1;
            if (!ignoreMultiplier) finalKnockbackMultiplier = (1 + knockbackMultiplier / 100f);

            playerCurrentAction = PlayerActions.STUNNED;
            playerStun.stunDuration = stunDuration;

            currentFrameNum = -1;

            rb.velocity = Vector2.zero;

            rb.AddForce(finalKnockbackMultiplier * knockbackForce, ForceMode2D.Impulse);
            knockbackMultiplier += knockbackIncrease;
        }
    }

    public void AddMeter(float value)
    {
        burstMeterValue += value;

        burstMeterValue = Mathf.Min(burstMeterValue, 1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            float intersectDistX = Mathf.Max(0, Mathf.Min(collision.collider.bounds.max.x, collision.otherCollider.bounds.max.x) - Mathf.Max(collision.collider.bounds.min.x, collision.otherCollider.bounds.min.x) + 2 * Physics2D.defaultContactOffset);

            if (intersectDistX != 0)
            {
                if (collision.collider.transform.position.x > collision.otherCollider.transform.position.x)
                {
                    collision.collider.transform.position += 0.3f * intersectDistX * Vector3.right;
                    collision.otherCollider.transform.position += 0.3f * intersectDistX * Vector3.left;
                }
                else if (collision.collider.transform.position.x < collision.otherCollider.transform.position.x)
                {
                    collision.collider.transform.position += 0.3f * intersectDistX * Vector3.left;
                    collision.otherCollider.transform.position += 0.3f * intersectDistX * Vector3.right;
                }
                else
                {
                    if (collision.collider.attachedRigidbody.velocity.y > 0)
                    {
                        collision.collider.transform.position += 0.3f * intersectDistX * Vector3.right;
                        collision.otherCollider.transform.position += 0.3f * intersectDistX * Vector3.left;
                    }
                    else if (collision.collider.attachedRigidbody.velocity.y < 0)
                    {
                        collision.collider.transform.position += 0.3f * intersectDistX * Vector3.left;
                        collision.otherCollider.transform.position += 0.3f * intersectDistX * Vector3.right;
                    }
                    else if (collision.otherCollider.attachedRigidbody.velocity.y > 0)
                    {
                        collision.collider.transform.position += 0.3f * intersectDistX * Vector3.left;
                        collision.otherCollider.transform.position += 0.3f * intersectDistX * Vector3.right;
                    }
                    else if (collision.otherCollider.attachedRigidbody.velocity.y < 0)
                    {
                        collision.collider.transform.position += 0.3f * intersectDistX * Vector3.right;
                        collision.otherCollider.transform.position += 0.3f * intersectDistX * Vector3.left;
                    }
                    else
                    {
                        collision.collider.transform.position += 0.3f * intersectDistX * Vector3.right;
                        collision.otherCollider.transform.position += 0.3f * intersectDistX * Vector3.left;
                    }
                }
            }
        }
    }
}
