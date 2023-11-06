using System.Linq;
using Unity.Mathematics;

namespace Character
{
	public enum MovementStates
	{
		NONE, Crouching, Blocking, MAX
	}

	public struct MovementState
	{
		public float SpeedMultiplier { get; set; }
		public bool IsActive { get; set; }
		public bool StrafeMovement { get; set; }
	}
	
	public class CharacterMovementStates
	{
		private readonly MovementState[] _movementStates;

		public CharacterMovementStates(float crouchSpeedMultiplier, float blockSpeedMultiplier)
		{
			_movementStates = new MovementState[(int) MovementStates.MAX];
			
			_movementStates[(int) MovementStates.Crouching].SpeedMultiplier = crouchSpeedMultiplier;
			_movementStates[(int) MovementStates.Blocking].SpeedMultiplier = blockSpeedMultiplier;
			_movementStates[(int) MovementStates.Blocking].StrafeMovement = true;
		}

		public void SetStateActive(MovementStates state)
		{
			_movementStates[(int) state].IsActive = true;
		}
		
		public void SetStateInactive(MovementStates state)
		{
			_movementStates[(int) state].IsActive = false;
		}

		/// <summary>
		/// Returns lowest active state speed multiplier
		/// </summary>
		public float GetSpeedMultiplier()
		{
			float multiplier = 1;
			foreach (var state in _movementStates)
			{
				if (state.IsActive)
				{
					multiplier = math.min(multiplier, state.SpeedMultiplier);
				}
			}

			return multiplier;
		}

		public bool HasStrafeMovement()
		{
			return _movementStates.Any(state => state is {IsActive: true, StrafeMovement: true});
		}
	}
}