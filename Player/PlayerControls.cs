using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;
using WolfRPG.Core.Statistics;

namespace Player
{
    [RequireComponent(typeof(PlayerCharacter))]
    public class PlayerControls : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float movementSpeed = 5;
        [SerializeField] private float jumpSpeed = 9;
        [SerializeField] private float gravity = 30;
        [SerializeField] private float inputCacheDuration = 0.2f;
        [SerializeField] private Transform playerCamera;
        [SerializeField] private float overWeightMultiplier = 0.4f;
        
        private PlayerCharacter _playerCharacter;
        private CharacterController _characterController;
        private Vector3 _velocity;
        private Vector2 _movementInput;
        private bool _jump;
        private bool _isDodging;
        private float _cachedAttackTime;
        private bool _hasCachedAttack;
        private bool _canDoSecondAttack;
        private bool _isOverweight;

        private static readonly int CanWalk = Animator.StringToHash("CanWalk");
        private static readonly int Jumping = Animator.StringToHash("Jumping");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        private static readonly int Dodge1 = Animator.StringToHash("Dodge");

        public static bool InputActive { get; private set; }

        private void Awake()
        {
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
        }

        private void OnDisable()
        {
            EventManager.OnJump -= OnJump;
            EventManager.OnMove -= OnMove;
            EventManager.OnDodge -= OnDodge;
            EventManager.OnAttack -= OnAttack;
            EventManager.OnBlock -= OnBlock;
            EventManager.OnInteract -= OnInteract;
        }

        private void Update()
        {
            if (InputActive && !_isDodging)
            {
                CheckOverWeight();
                Movement();
                Rotation();
                Actions();
            }
        }

        private void CheckOverWeight()
        {
            var maxWeight = _playerCharacter.Data.GetAttributeValue(Attribute.MaxCarryWeight);
            var currentWeight = _playerCharacter.Inventory.GetWeight();
            _isOverweight = currentWeight > maxWeight;
        }
        
        private void Movement()
        {
            if (!animator.GetBool(CanWalk) || _isDodging) return;
            
            if (_characterController.isGrounded)
            {
                animator.SetBool(Jumping, false);
                
                var forward = playerCamera.forward;
                var right = playerCamera.right;
                forward *= _movementInput.y;
                right *= _movementInput.x;
                _velocity = (forward + right) * movementSpeed;
                if (_isOverweight) _velocity *= overWeightMultiplier;
                
                if(_jump) Jump();
            }
            else
            {
                animator.SetFloat(Speed, 0);
            }

            var horizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);
            animator.SetFloat(Speed, horizontalVelocity.magnitude);
            _velocity.y -= gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }
        
        private void Actions()
        {
            if (_hasCachedAttack)
            {
                // Second attack
                if (animator.GetBool(IsAttacking))
                {
                    if (_canDoSecondAttack && Time.time - _cachedAttackTime < inputCacheDuration * 2 &&
                        _characterController.isGrounded)
                    {
                        _playerCharacter.Weapon.Attacking = false;
                        _cachedAttackTime = 0;
                        _playerCharacter.Attack();
                        _hasCachedAttack = false;
                    }
                }
                // First attack
                else if (Time.time - _cachedAttackTime < inputCacheDuration && _characterController.isGrounded)
                {
                    _cachedAttackTime = 0;
                    _playerCharacter.Attack();
                    _hasCachedAttack = false;
                }
            }
        }
        
        private IEnumerator Dodge()
        {
            if (_isOverweight) yield break;
            
            
            _isDodging = true;
            _playerCharacter.EndBlock();
			
            animator.SetTrigger(Dodge1);

            const float duration = 0.5f;
            var forward = _playerCharacter.graphic.forward;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                _velocity = forward * (movementSpeed * _playerCharacter.dodgeSpeed.Evaluate(t));
                _velocity.y -= gravity * Time.deltaTime;
                _characterController.Move(_velocity * Time.deltaTime);
                yield return null;
            }
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
        }
        
        public static void SetInputActive(bool enable)
        {
            InputActive = enable;
            Cursor.visible = !enable;
            Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
        private void Rotation()
        {
            if (_movementInput.magnitude > 0.1f)
            {
                var forward = playerCamera.forward;
                var right = playerCamera.right;
                forward *= _movementInput.y;
                right *= _movementInput.x;
                var direction = forward + right;
				
                var angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                _playerCharacter.graphic.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
            }
        }

        #region Input
        private void OnMove(InputAction.CallbackContext context)
        {
            if (InputActive == false) return;
            _movementInput = context.ReadValue<Vector2>();
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