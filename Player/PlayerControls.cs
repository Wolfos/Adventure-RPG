using System;
using System.Collections;
using Character;
using Crest;
using Items;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;
using WolfRPG.Core.Statistics;
using Attribute = WolfRPG.Core.Statistics.Attribute;

namespace Player
{
    [RequireComponent(typeof(PlayerCharacter))]
    public class PlayerControls : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float movementSpeed = 5;
        [SerializeField] private float swimSpeed = 2;
        [SerializeField] private float jumpSpeed = 9;
        [SerializeField] private float gravity = 30;
        [SerializeField] private float inputCacheDuration = 0.2f;
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float overWeightMultiplier = 0.4f;
        [SerializeField] private float transitionSmoothing = 0.3f;
        [SerializeField] private float swimAnimationSmoothing = 0.3f;
        [SerializeField, UnityEngine.Range(0, 0.9f)] private float inputSmoothing = 0.5f;
        [SerializeField, UnityEngine.Range(0, 0.9f)] private float rotationSmoothing = 0.5f;
        [SerializeField, UnityEngine.Range(0, 0.9f)] private float movementSmoothing = 0.5f;
        [SerializeField] private float movementDeadzone = 0.05f;
        [SerializeField] private float iFrameStart = 0.1f;
        [SerializeField] private float iFrameEnd = 0.4f;
        [SerializeField] private float graphicOffset = -1.074f;
        [SerializeField] private float swimStartTransition = -0.2f;
        [SerializeField] private float swimEndTransition = 0.2f;
        [SerializeField] private float swimOffset = 0.2f;
        [SerializeField] private Transform headBone;
        
        private PlayerCharacter _playerCharacter;
        private CharacterController _characterController;
        private Vector3 _velocity;
        private Vector2 _movementInput;
        private bool _jump;
        private bool _isDodging;
        private bool _isSwimming;
        private float _cachedAttackTime;
        private bool _hasCachedAttack;
        private bool _canDoSecondAttack;
        private bool _isOverweight;
        private float _lastSpeed;
        private float _lastHorizontalSpeed;
        private float _lastAngle;
        private Vector2 _smoothInputVelocity;
        private Vector3 _movementSmoothVelocity;
        private float _teleportTime;
        private SampleHeightHelper _sampleHeightHelper = new();
        private SampleFlowHelper _sampleFlowHelper = new();

        private static readonly int CanWalk = Animator.StringToHash("CanWalk");
        private static readonly int Jumping = Animator.StringToHash("Jumping");
        private static readonly int StartJump = Animator.StringToHash("StartJump");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int SidewaysSpeed = Animator.StringToHash("SidewaysSpeed");
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int SkipAttackAnticipation = Animator.StringToHash("SkipAttackAnticipation");
        private static readonly int Strafing = Animator.StringToHash("Strafing");
        private static readonly int Dodge1 = Animator.StringToHash("Dodge");
        private static readonly int DodgeX = Animator.StringToHash("DodgeX");
        private static readonly int DodgeY = Animator.StringToHash("DodgeY");

        public static bool InputActive { get; private set; }
        private static PlayerControls _instance;

        private void Awake()
        {
            _instance = this;
            _characterController = GetComponent<CharacterController>();
            _playerCharacter = GetComponent<PlayerCharacter>();
            _playerCharacter.animationEvents.onCanDoSecondAttack += OnCanDoSecondAttack;
            _playerCharacter.animationEvents.OnEndDoSecondAttack += OnEndDoSecondAttack;
            SetInputActive(true);
        }

        private void OnEnable()
        {
            EventManager.OnJump += OnJump;
            EventManager.OnMove += OnMove;
            EventManager.OnDodge += OnDodge;
            EventManager.OnAttack += OnAttack;
            EventManager.OnBlock += OnBlock;
            EventManager.OnInteract += OnInteract;
            EventManager.OnSprint += OnSprint;
        }

        private void OnDisable()
        {
            EventManager.OnJump -= OnJump;
            EventManager.OnMove -= OnMove;
            EventManager.OnDodge -= OnDodge;
            EventManager.OnAttack -= OnAttack;
            EventManager.OnBlock -= OnBlock;
            EventManager.OnInteract -= OnInteract;
            EventManager.OnSprint -= OnSprint;
        }

        private void Update()
        {
            if (InputActive && !_isDodging)
            {
                CheckOverWeight();
                Movement();
                Rotation();
#if UNITY_EDITOR
                if (Time.time < 0.5f) return; // Prevent accidentally hitting NPCs every time we hit play in the editor
#endif
                Actions();
            }
        }

        private void CheckOverWeight()
        {
            var maxWeight = _playerCharacter.GetAttributeValue(Attribute.MaxCarryWeight);
            var currentWeight = _playerCharacter.Inventory.GetWeight();
            _isOverweight = currentWeight > maxWeight;
        }

        private void SwimMovement(float distanceFromWaterSurface, float headDistanceFromWaterSurface)
        {
            var localVelocity = new Vector2();
            var forward = playerCamera.forward;
            var right = playerCamera.right;
            forward *= _movementInput.y;
            right *= _movementInput.x;
            localVelocity.x = _movementInput.x;
            localVelocity.y = _movementInput.y;
                
            _velocity = (forward + right) * swimSpeed;
            localVelocity *= swimSpeed;
            if (_isOverweight)
            {
                _velocity *= overWeightMultiplier;
                localVelocity *= overWeightMultiplier;
            }
            _velocity *= _playerCharacter.SpeedMultiplier;
            localVelocity *= _playerCharacter.SpeedMultiplier;
            
            var speed = Mathf.Lerp(_lastSpeed, localVelocity.magnitude, swimAnimationSmoothing);

            _lastSpeed = speed;
            _lastHorizontalSpeed = 0;
            
            animator.SetFloat(Speed, speed);
            animator.SetFloat(SidewaysSpeed, 0);
            
            var smoothVelocity = Vector3.SmoothDamp(_characterController.velocity, _velocity,
                ref _movementSmoothVelocity, movementSmoothing);
            
            smoothVelocity.y -= (headDistanceFromWaterSurface + swimOffset) * 4;
            
            _sampleFlowHelper.Init(transform.position, 1);
            _sampleFlowHelper.Sample(out var flow);
            smoothVelocity.x += flow.x;
            smoothVelocity.z += flow.y;

            
            _characterController.Move(smoothVelocity * Time.deltaTime);
        }
        
        private void Movement()
        {
            if (Time.time - _teleportTime < 0.1f) return;
            
            // Water
            if (OceanRenderer.Instance != null)
            {
                _sampleHeightHelper.Init(transform.position, 1, true);
                var height = OceanRenderer.Instance.SeaLevel;
                _sampleHeightHelper.Sample(out Vector3 disp, out var normal, out var waterSurfaceVel);
                height += disp.y;
                var distanceFromWater = transform.position.y - height;
                var headWaterDistance = headBone.position.y - height;

                if (_isSwimming && _characterController.isGrounded)
                {
                    _isSwimming = distanceFromWater + swimEndTransition < 0;
                    if (_isSwimming == false)
                    {
                        PlayerCamera.SetState(CameraState.Default);
                    }
                }
                else if (_isSwimming == false)
                {
                    _isSwimming = distanceFromWater + swimStartTransition < 0;
                    if (_isSwimming)
                    {
                        PlayerCamera.SetState(CameraState.Swimming);
                    }

                }

                animator.SetBool(Swimming, _isSwimming);

                if (_isSwimming)
                {
                    animator.SetBool(Jumping, false);
                    SwimMovement(distanceFromWater, headWaterDistance);
                    return;
                }
            }
            else // Oceanrenderer.instance is null
            {
                _isSwimming = false;
                animator.SetBool(Swimming, _isSwimming);
            }
            
            if (_characterController.isGrounded)
            {
                animator.SetBool(Jumping, false);
            }

            if (_isDodging) return;

            var canWalk = animator.GetBool(CanWalk);
            var localVelocity = new Vector2();
            if (canWalk)
            {
                if (_characterController.isGrounded)
                {
                    var forward = playerCamera.forward;
                    var right = playerCamera.right;
                    forward *= _movementInput.y;
                    right *= _movementInput.x;
                    localVelocity.x = _movementInput.x;
                    localVelocity.y = _movementInput.y;

                    _velocity = (forward + right) * movementSpeed;
                    localVelocity *= movementSpeed;
                    if (_isOverweight)
                    {
                        _velocity *= overWeightMultiplier;
                        localVelocity *= overWeightMultiplier;
                    }

                    _velocity *= _playerCharacter.SpeedMultiplier;
                    localVelocity *= _playerCharacter.SpeedMultiplier;

                    if (_jump) Jump();
                }
                else
                {
                    animator.SetFloat(Speed, 0);
                }
            }
            else
            {
                _velocity = Vector3.zero;
            }


            _velocity.y -= gravity * Time.deltaTime;

            var smoothVelocity = Vector3.SmoothDamp(_characterController.velocity, _velocity,
                ref _movementSmoothVelocity, movementSmoothing);
            var horizontalVelocity = new Vector3(smoothVelocity.x, 0, smoothVelocity.z);
            
            // if (smoothVelocity.sqrMagnitude < 0.1f)
            // {
            //     smoothVelocity = Vector3.zero;
            // }
           
            if (_playerCharacter.StrafeMovement)
            {
                animator.SetBool(Strafing, true);
                
                var speed = Mathf.Lerp(_lastSpeed, localVelocity.y, transitionSmoothing);
                var sidewaysSpeed = Mathf.Lerp(_lastHorizontalSpeed, localVelocity.x, transitionSmoothing);
                if (_characterController.isGrounded)
                {
                    _lastSpeed = speed;
                    _lastHorizontalSpeed = sidewaysSpeed;
                }
                animator.SetFloat(Speed, speed);
                animator.SetFloat(SidewaysSpeed, sidewaysSpeed);
            }
            else
            {
                animator.SetBool(Strafing, false);
                
                var speed = Mathf.Lerp(_lastSpeed, horizontalVelocity.magnitude, transitionSmoothing);
                var sidewaysSpeed = 0;
                if (_characterController.isGrounded)
                {
                    _lastSpeed = speed;
                    _lastHorizontalSpeed = sidewaysSpeed;
                }
                animator.SetFloat(Speed, speed);
                animator.SetFloat(SidewaysSpeed, sidewaysSpeed);
            }
            
            
            if (animator.GetBool(RootMotion))
            {
                smoothVelocity = animator.velocity;
            }
            _characterController.Move(smoothVelocity * Time.deltaTime);
        }

        public void Teleport(Vector3 position)
        {
            transform.position = position;
            _teleportTime = Time.time;
        }

        private static readonly int RootMotion = Animator.StringToHash("RootMotion");
        private static readonly int Swimming = Animator.StringToHash("Swimming");

        private void Actions()
        {
            if (_isSwimming) return;
            
            if (_hasCachedAttack)
            {
                // Second attack
                if (animator.GetBool(IsAttacking))
                {
                    if (Time.time - _cachedAttackTime < inputCacheDuration * 4 && _isDodging == false) 
                    {
                        if(_playerCharacter.Weapon) _playerCharacter.Weapon.Attacking = false;
                        _cachedAttackTime = 0;
                        _playerCharacter.Attack();
                        _hasCachedAttack = false;
                    }
                }
                // First attack
                else if (Time.time - _cachedAttackTime < inputCacheDuration && _characterController.isGrounded && _isDodging == false)
                {
                    _cachedAttackTime = 0;
                    animator.SetBool(SkipAttackAnticipation, true);
                    _playerCharacter.Attack();
                    _hasCachedAttack = false;
                    animator.SetBool(RootMotion, true);
                }
            }
        }
        
        private IEnumerator Dodge()
        {
            if (_isOverweight) yield break;
            if (_playerCharacter.Weapon != null && _playerCharacter.Weapon.Attacking) yield break;
            if (_playerCharacter.CanDodge() == false) yield break; // TODO: Flash stamina bar
            
            var athleticsLevel = _playerCharacter.GetSkillValue(Skill.Athletics);
            _playerCharacter.ChangeStamina(-CharacterBase.DodgeStaminaCost - athleticsLevel * 0.1f);
            _playerCharacter.AddDodgeXP();
            
            _isDodging = true;
            

            const float duration = 0.5f;
            
            var direction = _playerCharacter.graphic.forward;
            animator.SetTrigger(Dodge1);
            
            if (_playerCharacter.StrafeMovement)
            {
                direction = _playerCharacter.graphic.TransformDirection(new(_movementInput.x, 0, _movementInput.y));

                // Decide which directional animation to play
                if (_movementInput.y < 0 && math.abs(_movementInput.x) < 0.6f) // Backwards
                {
                    animator.SetFloat(DodgeX, 0);
                    animator.SetFloat(DodgeY, -1);
                }
                else if (_movementInput.y > 0 && math.abs(_movementInput.x) < 0.6f) // Forwards
                {
                    animator.SetFloat(DodgeX, 0);
                    animator.SetFloat(DodgeY, 1);
                }
                else if (_movementInput.x < 0) // Left
                {
                    animator.SetFloat(DodgeX, -1);
                    animator.SetFloat(DodgeY, 0);
                }
                else // Right
                {
                    animator.SetFloat(DodgeX, 1);
                    animator.SetFloat(DodgeY, 0);
                }
            }
            else
            {
                animator.SetFloat(DodgeX, 0);
                animator.SetFloat(DodgeY, 1);
                //_playerCharacter.EndBlock();
            }


            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                _velocity = direction * (movementSpeed * _playerCharacter.dodgeSpeed.Evaluate(t));
                _velocity.y -= gravity * Time.deltaTime;
                _characterController.Move(_velocity * Time.deltaTime);

                if (t > iFrameEnd)
                {
                    _playerCharacter.SetVulnerable();
                }
                else if (t > iFrameStart)
                {
                    _playerCharacter.SetInvulnerable();
                }
                yield return null;
            }
            
            _playerCharacter.SetVulnerable();
            
            _isDodging = false;
        }
        
        private void OnCanDoSecondAttack()
        {
            _canDoSecondAttack = true;
        }

        private void OnEndDoSecondAttack()
        {
            _canDoSecondAttack = false;
        }

        private void Jump()
        {
            if (_isOverweight) return;
            
            _jump = false;
            _velocity.y = jumpSpeed;
            animator.SetBool(Jumping, true);
            animator.SetTrigger(StartJump);
        }
        
        public static void SetInputActive(bool enable)
        {
            if (enable == false && Time.timeScale != 0)
            {
                _instance.animator.SetFloat(Speed, 0);
                _instance._movementInput = Vector2.zero;
            }
            
            InputActive = enable;
            Cursor.visible = !enable;
            Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        private void Rotation()
        {
            var strafe = _playerCharacter.StrafeMovement;
            if (_movementInput.magnitude > 0.1f || strafe)
            {
                var forward = playerCamera.forward;
                var right = playerCamera.right;
                if (strafe)
                {
                    right *= 0;
                }
                else
                {
                    forward *= _movementInput.y;
                    right *= _movementInput.x;
                }

                var direction = forward + right;
				
                var angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                var sidewaysSpeed = (angle - _lastAngle) * Time.deltaTime;
                if (strafe == false)
                {
                    // TODO: Reimplement lean
                    //animator.SetFloat(SidewaysSpeed, sidewaysSpeed);
                }

                
                var targetRotation = Quaternion.Euler(new(0, angle, 0));
                _playerCharacter.graphic.rotation = Quaternion.Lerp(_playerCharacter.graphic.rotation, targetRotation,
                    1 - rotationSmoothing);
                _lastAngle = angle;
            }
        }

        #region Input
        private void OnMove(InputAction.CallbackContext context)
        {
            if (InputActive == false) return;
            _movementInput = Vector2.SmoothDamp(_movementInput, context.ReadValue<Vector2>(), ref _smoothInputVelocity, inputSmoothing * 0.01f);
            if (_movementInput.magnitude < movementDeadzone)
            {
                _movementInput = Vector2.zero;
                _playerCharacter.StopSprint();
            }
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (InputActive == false) return;
            if (context.phase == InputActionPhase.Performed)
            {
                _jump = true; // Needs to be synced with Update, so just set a bool here
            }
        }
        
        private void OnDodge(InputAction.CallbackContext context)
        {
            if (InputActive == false) return;
            if (_isDodging || !_characterController.isGrounded) return;
            
            if (context.phase == InputActionPhase.Performed)
            {
                StartCoroutine(Dodge());
            }
        }
		
        private void OnAttack(InputAction.CallbackContext context)
        {
            if (InputActive == false) return;
            
            if (context.phase == InputActionPhase.Performed)
            {
                _cachedAttackTime = Time.time;
                _hasCachedAttack = true;
            }
        }
		
        private void OnBlock(InputAction.CallbackContext context)
        {
            if (InputActive == false) return;
            
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    _playerCharacter.StartBlock();
                    break;
                case InputActionPhase.Canceled:
                    _playerCharacter.EndBlock();
                    break;
            }
        }

        private void OnSprint(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    if (_playerCharacter.StrafeMovement)
                    {
                        return;
                    }
                    if (_movementInput.magnitude > movementDeadzone)
                    {
                        _playerCharacter.StartSprint();
                    }

                    break;
                case InputActionPhase.Canceled:
                    _playerCharacter.StopSprint();
                    break;
            }
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            if (InputActive == false) return;
            
            if (context.phase == InputActionPhase.Canceled)
            {
                _playerCharacter.Interact();
            }
        }
        #endregion
    }
}