using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat;
using Data;
using Items;
using UI;
using UnityEngine;
using Character;
using Utility;

namespace Player
{
	public class PlayerCharacter : CharacterBase
	{	
		[HideInInspector] public CharacterController characterController;
		
		[Space(30)] [SerializeField] private float movementSpeed;
		[SerializeField] private float jumpSpeed;
		[SerializeField] private AnimationCurve dodgeSpeed;
		[SerializeField] private float gravity;
		[SerializeField] private Texture2D defaultCursor, aimCursor;

		private PlayerCamera playerCamera;
		public static bool inputActive = true;
		private Vector3 velocity;
		private static readonly int CanWalk = Animator.StringToHash("CanWalk");
		private bool _dodging;

		private void Awake()
		{
			base.Awake();
			SystemContainer.Register(this);
		}
		
		private void Start()
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			//Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
			characterController = GetComponent<CharacterController>();
			StartCoroutine(EnableMovement());

			playerCamera = SystemContainer.GetSystem<PlayerCamera>();

			base.Start();
		}

		private void OnDestroy()
		{
			SystemContainer.UnRegister<PlayerCharacter>();
		}

		public void StartQuest(Quest quest)
		{
			if (HasQuest(quest)) return;
			
			var q = Instantiate(quest);
			q.name = quest.name;
			data.quests.Add(q);
			data.questProgress.Add(quest.progress);
		}

		public bool HasQuest(Quest quest)
		{
			return data.quests.Any(x => x.name == quest.name);
		}

		private IEnumerator EnableMovement()
		{
			yield return new WaitForSeconds(1);
			inputActive = true;
		}

		private void Update()
		{
			if (inputActive && !_dodging && !IsInHitRecoil)
			{
				Actions();
				Movement();
				Rotation();
			}

			Menus();
			
			
			base.Update();
		}

		private void Movement()
		{
			if (!animator.GetBool(CanWalk)) return;
			
			if (characterController.isGrounded)
			{
				animator.SetBool("Jumping", false);
				var input = InputMapper.GetMovement();
				input = Vector3.ClampMagnitude(input, 1);

				var cameraTransform = playerCamera.transform;
				var forward = cameraTransform.forward;
				var right = cameraTransform.right;
				forward *= input.z;
				right *= input.x;

				velocity = (forward + right) * movementSpeed;
				
				if (InputMapper.JumpButton())
				{
					Jump();
					animator.SetBool("Jumping", true);
				}
			}
			else
			{
				animator.SetFloat("Speed", 0);
			}

			var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
			animator.SetFloat("Speed", horizontalVelocity.magnitude);
			velocity.y -= gravity * Time.deltaTime;
			characterController.Move(velocity * Time.deltaTime);
		}

		private void Jump()
		{
			velocity.y = jumpSpeed;
		}

		public static void SetInputActive(bool enable)
		{
			inputActive = enable;
			Cursor.visible = !enable;
			Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
		}

		private void Menus()
		{
			// Open or close inventory
			if (InputMapper.InventoryButton())
			{
				SetInputActive(!inputActive);
				PlayerMenu.ToggleActive();
			}
			
			// Open or close pause menu
			if (InputMapper.PauseButton())
			{
				SetInputActive(!inputActive);
				PauseMenu.ToggleActive();
			}
		}

		

		protected override void DeathAnimationStarted()
		{
			inputActive = false;
		}

		protected override void DeathAnimationFinished()
		{
			inputActive = true;
			SystemContainer.GetSystem<SaveGameManager>().LoadSaveGame();
		}

		private void Actions()
		{
			// Attack
			if (InputMapper.AttackButton() && characterController.isGrounded)
			{
				Attack();
			}
			
			// General interaction
			if (InputMapper.InteractionButton())
			{
				Interact();
			}

			// Dodge
			if (InputMapper.DodgeButton() && !_dodging && characterController.isGrounded)
			{
				StartCoroutine(Dodge());
			}
		}
		

		private IEnumerator Dodge()
		{
			_dodging = true;
			animator.SetTrigger("Dodge");
			
			const float duration = 0.5f;
			var forward = graphic.forward;
			for (float t = 0; t < duration; t += Time.deltaTime)
			{
				velocity = forward * movementSpeed * dodgeSpeed.Evaluate(t);
				velocity.y -= gravity * Time.deltaTime;
				characterController.Move(velocity * Time.deltaTime);
				yield return null;
			}
			_dodging = false;

		}

		public static void LockInput()
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			inputActive = false;
			Time.timeScale = 0;
		}
		
		public static void UnlockInput()
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			inputActive = true;
			Time.timeScale = 1;
		}

		private void Rotation()
		{
			var input = InputMapper.GetMovement();
			if (input.magnitude > 0.1f)
			{
				var cameraTransform = playerCamera.transform;
				var forward = cameraTransform.forward;
				var right = cameraTransform.right;
				forward *= input.z;
				right *= input.x;
				var direction = forward + right;
				
				float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
				graphic.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
			}
		}
	}
}