using JetBrains.Annotations;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace TarodevController
{
    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// I have a premium version on Patreon, which has every feature you'd expect from a polished controller. Link: https://www.patreon.com/tarodev
    /// You can play and compete for best times here: https://tarodev.itch.io/extended-ultimate-2d-controller
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/tarodev
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour, IPlayerController
    {

        public void EnableCollisions()
        {
            Physics2D.IgnoreCollision(_col, _platform, false);
            Physics2D.IgnoreCollision(_col, _platform2, false);
            Physics2D.IgnoreCollision (_col, _platform3, false);
            Physics2D.IgnoreCollision ( _col, _platform4, false);
            Physics2D.IgnoreCollision(_col, _platform5, false);
        }
        public void DisableCollisions()
        {
            Physics2D.IgnoreCollision(_col, _platform, true);
            Physics2D.IgnoreCollision(_col, _platform2, true);
            Physics2D.IgnoreCollision( _col, _platform3, true);
            Physics2D.IgnoreCollision(_col, _platform4, true);
            Physics2D.IgnoreCollision(_col, _platform5, true);
        }




        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Platform"))
            {
                isOnPlatform = false;
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {


            if (collision.gameObject.tag == "nPlatform")
            {

                isNearPlatform = true;
                Debug.Log("PlatformUWU");
                
            }
            else
            {
                isNearPlatform = false;
            }
               
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {

           
            if (collision.gameObject.CompareTag("Platform"))
            {
                isOnPlatform = true;
                Debug.Log("Enter");
            }
            if (collision.gameObject.tag == "nGround")
            {
                EnableCollisions();
            }

            if (collision.gameObject.tag == "TopWall")
            {
                _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);
            }
            
        }




        private IEnumerator DisablePlayerCollider(float disableTime)
            {

            DisableCollisions();
            yield return new WaitForSeconds(disableTime);
            EnableCollisions();
            }

        [SerializeField] private float doubleClickTimeWindow = 0.3f; 

    private float lastClickTime;

        [SerializeField] private ScriptableStats _stats;
        private Rigidbody2D _rb;
        private CapsuleCollider2D _col;
        [SerializeField] BoxCollider2D _platform;
        [SerializeField] BoxCollider2D _platform2;
        [SerializeField] BoxCollider2D _platform3;
        [SerializeField] BoxCollider2D _platform4;
        [SerializeField] BoxCollider2D _platform5;
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;
        public bool isOnPlatform;
        public bool isNearPlatform;

        private bool canDash = true;
        private bool isDashing;
        private float dashingPower = 24f;
        private float dashingTime = 0.2f;
        private float HorizonalDashPower = 45f;
        private float VerticalDashPower = 24.0f;



        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        #endregion

        private float _time;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<CapsuleCollider2D>();

            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;

        }
 

        private void Update()
        {

            _time += Time.deltaTime;

            if (_grounded==true)
                canDash = true;



            GatherInput();

        }

        private IEnumerator Dash()
        {
            Debug.Log("Dash check v3");
            canDash = false;
            isDashing = true;
            float originalGravity = _rb.gravityScale;
            _rb.gravityScale = 0;
            
            _frameVelocity = new Vector2(HorizonalDashPower, transform.localScale.y * VerticalDashPower);
            Debug.Log(_rb.velocity);
            Debug.Log(HorizonalDashPower);
            if (_grounded == true)
            {
                Debug.Log("Dash break");
                isDashing = false;
                _rb.gravityScale = originalGravity;
                yield break;
            }
            yield return new WaitForSeconds(dashingTime);
            
            _rb.gravityScale = originalGravity;
            isDashing = false;
            
        }

        private void GatherInput()
        {
            _frameInput = new FrameInput
            {
                JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
                JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
                //DropDown = Input.GetKey(KeyCode.S) && Input.GetButtonDown("Jump"),
                DoubleDown = Input.GetKey(KeyCode.S) || Input.GetKeyDown(KeyCode.S),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
                DashHorizonal = Input.GetKeyDown(KeyCode.LeftShift),
                //DashVerticalUp = Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.W),
                //DashVerticalDown = Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.S),
                DashAngleDownR = Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.S),
                DashAngleDownL = Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.S),
                DashAngleUpR = Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D),
                DashAngleUpL = Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A),
            };


            if (_frameInput.DashAngleDownR)
            {
                if (canDash == true)
                {
                    HorizonalDashPower = 30.0f;
                    VerticalDashPower = -30.0f;
                    StartCoroutine(Dash());
                }
            }

            if (_frameInput.DashAngleDownL)
            {
                if (canDash == true)
                {
                    HorizonalDashPower = -30.0f;
                    VerticalDashPower = -30.0f;
                    StartCoroutine(Dash());
                }
            }

            if (_frameInput.DashAngleUpR)
            {
                if (canDash == true)
                {
                    HorizonalDashPower = 20.0f;
                    VerticalDashPower = 20.0f;
                    StartCoroutine(Dash());
                }
            }

            if (_frameInput.DashAngleUpL)
            {
                if (canDash == true)
                {
                    HorizonalDashPower = -20.0f;
                    VerticalDashPower = 20.0f;
                    StartCoroutine(Dash());
                }
            }

            if (_frameInput.DashHorizonal)
            {
                Debug.Log("Dash Check");
                if (canDash == true)
                {
                    Debug.Log("dash check v2");
                    if (_frameInput.Move.x > 0)
                    {

                        HorizonalDashPower = 40.0f;
                        VerticalDashPower = 0.0f;
                        StartCoroutine(Dash());
                        

                    }
                    else if (_frameInput.Move.x == 0) { }
                    else
                    {
                        HorizonalDashPower = -40.0f;
                        VerticalDashPower = 0.0f;
                        StartCoroutine(Dash());
                    }
                    
                    
                }
            }

            if (_frameInput.DoubleDown)
            {
                /*if (isNearPlatform == true)
                {
                    StartCoroutine(DisablePlayerCollider(0.15f));
                }
                Debug.Log("Near");
                if (Input.GetKeyDown(KeyCode.S))
                {
                    Debug.Log("Fucker");
                }*/

                if (isOnPlatform == true)
                {
                    {
                        if (Input.GetKeyDown(KeyCode.S))
                        {
                            // Calculate the time elapsed since the last click
                            float timeSinceLastClick = Time.time - lastClickTime;

                            Debug.Log("down");



                            // If the time elapsed is within the defined window, it's a double-click
                            if (timeSinceLastClick <= doubleClickTimeWindow)
                            {

                                StartCoroutine(DisablePlayerCollider(0.15f));
                            }

                            // Update the last click time for the next potential click
                            lastClickTime = Time.time;
                        }
                    }
                }

                if (isNearPlatform == true)
                {
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        // Calculate the time elapsed since the last click
                        float timeSinceLastClick = Time.time - lastClickTime;

                        Debug.Log("fall");



                        // If the time elapsed is within the defined window, it's a double-click
                        if (timeSinceLastClick <= doubleClickTimeWindow)
                        {

                            StartCoroutine(DisablePlayerCollider(0.15f));
                        }

                        // Update the last click time for the next potential click
                        lastClickTime = Time.time;
                    }
                }
            }

            if (_frameInput.DropDown)



            {
                if (isOnPlatform == true)
                {
                    StartCoroutine(DisablePlayerCollider(0.15f));
                }
            }



            else if (_frameInput.JumpDown)
               {
                    _jumpToConsume = true;
                    _timeJumpWasPressed = _time;
               }


            if (_stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }

            

            
        }

        private void FixedUpdate()
        {
            CheckCollisions();

            HandleJump();
            HandleDirection();
            HandleGravity();
            ApplyMovement(); 
            
            

        }

        #region Collisions
        
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        private void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            bool platformHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, _stats.GrounderDistance, ~_stats.PlayerLayer);
            // bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, _stats.GrounderDistance, ~_stats.PlayerLayer);
            
            // Hit a Ceiling
            //if (ceilingHit && ) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit && !isNearPlatform)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion


        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + _stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + _stats.CoyoteTime;



        private void HandleJump()
        {



            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;



            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote) ExecuteJump();

            _jumpToConsume = false;
        }

        private void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = _stats.JumpPower;
            Jumped?.Invoke();
        }



        #endregion

        #region Horizontal

        private void HandleDirection()
        {
            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);

            }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * _stats.MaxSpeed, _stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = _stats.GroundingForce;
            }
            else if (isDashing == false)
            {
                {
                    var inAirGravity = _stats.FallAcceleration;
                    if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= _stats.JumpEndEarlyGravityModifier;
                    _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
                }
            }
            else { _rb.gravityScale = 0; }
        }

        #endregion
        
        private void ApplyMovement() => _rb.velocity = _frameVelocity;

        

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public bool DropDown;
        public bool DoubleDown;
        public bool DashHorizonal;
        public bool DashVerticalUp;
        public bool DashVerticalDown;
        public bool DashAngleUp;
        public bool DashAngleDownR;
        public bool DashAngleUpR;
        public bool DashAngleDownL;
        public bool DashAngleUpL;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;

        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }
}