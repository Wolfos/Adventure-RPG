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
	public class Player : CharacterBase
	{	
		[HideInInspector] public CharacterController characterController;
		
		[Space(30)] [SerializeField] private float movementSpeed;
		[SerializeField] private float jumpSpeed;
		[SerializeField] private float gravity;
		[SerializeField] private Texture2D defaultCursor, aimCursor;

		private PlayerCamera playerCamera;
		public static bool disableMovement = true;
		private Vector3 velocity;
		private static readonly int CanWalk = Animator.StringToHash("CanWalk");

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
			SystemContainer.UnRegister<Player>();
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
			disableMovement = false;
		}

		private void Update()
		{
			if (!disableMovement)
			{
				Actions();
				Movement();
				Rotation();
				SetCursor();
			}

			Menus();
			
			
			base.Update();
		}

		private void Movement()
		{
			if (!animator.GetBool(CanWalk)) return;
			
			if (characterController.isGrounded)
			{
				var input = InputMapper.GetMovement();
				input = Vector3.ClampMagnitude(input, 1);

				var cameraTransform = playerCamera.transform;
				var forward = cameraTransform.forward;
				var right = cameraTransform.right;
				forward *= input.z;
				right *= input.x;

				velocity = (forward + right) * movementSpeed;
				animator.SetFloat("Speed", velocity.magnitude);
				if (InputMapper.JumpButton()) velocity.y = jumpSpeed;
			}
			else
			{
				animator.SetFloat("Speed", 0);
			}
			
			velocity.y -= gravity * Time.deltaTime;
			characterController.Move(velocity * Time.deltaTime);
		}

		private void Menus()
		{
			// Open or close inventory
			if (InputMapper.InventoryButton())
			{
				disableMovement = !disableMovement;
				Cursor.visible = disableMovement;
				Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
				PlayerMenu.ToggleActive();
			}
			
			// Open or close pause menu
			if (InputMapper.PauseButton())
			{
				disableMovement = !disableMovement;
				Cursor.visible = disableMovement;
				Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
				PauseMenu.ToggleActive();
			}
		}

		

		protected override void DeathAnimationStarted()
		{
			disableMovement = true;
		}

		protected override void DeathAnimationFinished()
		{
			disableMovement = false;
			SystemContainer.GetSystem<SaveGameManager>().LoadSaveGame();
		}

		private void Actions()
		{
			// Attack
			if (InputMapper.AttackButton())
			{
				Attack();
			}
			
			// General interaction
			if (InputMapper.InteractionButton())
			{
				Interact();
			}

		}

		private void SetCursor()
		{
//			if(Input.GetButton("Aim"))
//			{
//				Cursor.SetCursor(aimCursor, new Vector2(aimCursor.width / 2, aimCursor.height / 2), CursorMode.Auto);
//			}
//			else if (Input.GetButtonUp("Aim"))
//			{
//				Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
//			}
		}

		private void Rotation()
		{
			if(InputMapper.AimButtonHeld())
			{
				float angle = 0;
				if (InputMapper.usingController)
				{
					var input = InputMapper.GetRightStick();
					angle = Mathf.Atan2(input.y, -input.x) * Mathf.Rad2Deg;

					if (input.sqrMagnitude < 0.1f)
					{
						var input2 = InputMapper.GetMovement();
						angle = Mathf.Atan2(input2.x, input2.z) * Mathf.Rad2Deg;
					}
				}
				else
				{
					Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
					Vector2 relative = (Vector2) Input.mousePosition - screenPos;
					angle = Mathf.Atan2(relative.x, relative.y) * Mathf.Rad2Deg;
				}

				graphic.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
			}
			else
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
}