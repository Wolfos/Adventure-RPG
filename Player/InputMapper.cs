using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
	public static class InputMapper
	{
		public static bool usingController;

		public static Vector3 GetMovement()
		{
			Vector3 input = Vector3.zero;
			input.x = Input.GetAxis("HorizontalX");
			input.z = Input.GetAxis("VerticalX");

			bool controller = true;
			if (Mathf.Approximately(input.sqrMagnitude, 0))
			{
				input.x = Input.GetAxis("Horizontal");
				input.z = Input.GetAxis("Vertical");
				controller = false;
			}
			else
			{
				usingController = true;
			}

			if (!controller && !Mathf.Approximately(input.sqrMagnitude, 0))
			{
				usingController = false;
			}

			return input;
		}

		public static Vector2 GetCameraMovement()
		{
			var input = Vector2.zero;
			input = GetRightStick();
			if (usingController == false)
			{
				input.x = -Input.GetAxis("Mouse X");
				input.y = Input.GetAxis("Mouse Y");
			}

			return input;
		}

		public static Vector2 GetRightStick()
		{
			var input = Vector2.zero;
			input.x = Input.GetAxis("RightStickX");
			input.y = Input.GetAxis("RightStickY");

			if (!Mathf.Approximately(input.sqrMagnitude, 0))
			{
				usingController = true;
			}

			return input;
		}

		public static bool InventoryButton()
		{
			if (Input.GetButtonDown("InventoryX"))
			{
				usingController = true;
				return true;
			}
			
			if (Input.GetButtonDown("Inventory"))
			{
				usingController = false;
				return true;
			}

			return false;
		}
		
		public static bool InventoryButtonHeld()
		{
			if (Input.GetButton("InventoryX"))
			{
				usingController = true;
				return true;
			}
			
			if (Input.GetButton("Inventory"))
			{
				usingController = false;
				return true;
			}

			return false;
		}
		
		public static bool PauseButton()
		{
			if (Input.GetButtonDown("PauseX"))
			{
				usingController = true;
				return true;
			}
			
			if (Input.GetButtonDown("Pause"))
			{
				usingController = false;
				return true;
			}

			return false;
		}

		public static bool JumpButton()
		{
			if (Input.GetButtonDown("JumpX"))
			{
				usingController = true;
				return true;
			}
			
			if (Input.GetButtonDown("Jump"))
			{
				usingController = false;
				return true;
			}

			return false;
		}

		public static bool InteractionButton()
		{
			if (Input.GetButtonDown("InteractionX"))
			{
				usingController = true;
				return true;
			}
			
			if (Input.GetButtonDown("Interaction"))
			{
				usingController = false;
				return true;
			}

			return false;
		}

		public static bool InteractionButtonHeld()
		{
			if (Input.GetButton("InteractionX"))
			{
				usingController = true;
				return true;
			}
			
			if (Input.GetButton("Interaction"))
			{
				usingController = false;
				return true;
			}

			return false;
		}
		
		public static bool DropButton()
		{
			if (Input.GetButtonDown("DropX"))
			{
				usingController = true;
				return true;
			}
			
			if (Input.GetButtonDown("Drop"))
			{
				usingController = false;
				return true;
			}

			return false;
		}
		
		public static bool DropButtonHeld()
		{
			if (Input.GetButton("DropX"))
			{
				usingController = true;
				return true;
			}
			
			if (Input.GetButton("Drop"))
			{
				usingController = false;
				return true;
			}

			return false;
		}

		public static bool AimButtonHeld()
		{
			if(Input.GetAxis("AimX") > 0.5f)
			{
				usingController = true;
				return true;
			}
			
			if (Input.GetButton("Aim"))
			{
				usingController = false;
				return true;
			}

			return false;
		}

		private static bool hasAttacked;
		public static bool AttackButton()
		{
			if (hasAttacked && Input.GetAxis("AttackX") < 0.1f)
			{
				hasAttacked = false;
			}
			
			if(!hasAttacked && Input.GetAxis("AttackX") > 0.5f)
			{
				usingController = true;
				hasAttacked = true;
				return true;
			}
			
			if (Input.GetButtonDown("Attack"))
			{
				usingController = false;
				return true;
			}

			return false;
		}

		public static bool MenuLeft()
		{
			return Input.GetKeyDown("joystick button 4");
		}

		public static bool MenuRight()
		{
			return Input.GetKeyDown("joystick button 5");
		}
	}
}