using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using Data;
using Items;
using UI;
using UnityEngine;
using NPC;
using Utility;

namespace Player
{
	public class Player : Character
	{	
		[HideInInspector] public CharacterController characterController;
		
		[Space(30)] [SerializeField] private float movementSpeed;
		[SerializeField] private float jumpSpeed;
		[SerializeField] private float gravity;
		[SerializeField] private Texture2D defaultCursor, aimCursor;
		
		
		public static bool disableMovement = true;
		private Vector3 velocity;
		
		

		private void Start()
		{
			Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
			characterController = GetComponent<CharacterController>();
			StartCoroutine(EnableMovement());

			base.Start();
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
			if (characterController.isGrounded)
			{
				var input = InputMapper.GetMovement();
				input = Vector3.ClampMagnitude(input, 1);
				velocity = input * movementSpeed;
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
				Inventory.ToggleActive(inventory);
			}
			
			// Open or close pause menu
			if (InputMapper.PauseButton())
			{
				disableMovement = !disableMovement;
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
					float angle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg;
					graphic.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
				}
			}
		}
	}
}