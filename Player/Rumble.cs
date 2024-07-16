using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
	public class Rumble: MonoBehaviour
	{
		private static float _leftRumble;
		private static float _rightRumble;
		private static float _rumbleDuration;
		private static float _rumbleStartTime;

		private void Awake()
		{
			_rumbleStartTime = 0;
		}

		private void Update()
		{
			if (Time.time < _rumbleStartTime + _rumbleDuration)
			{
				Gamepad.current?.SetMotorSpeeds(_leftRumble, _rightRumble);
			}
			else
			{
				Gamepad.current?.SetMotorSpeeds(0, 0);
			}
		}

		public static void SetMotorSpeeds(float left, float right, float duration)
		{
			if (InputMapper.UsingController == false) return;
			
			_leftRumble = left;
			_rightRumble = right;
			_rumbleDuration = duration;
			_rumbleStartTime = Time.time;
		}
	}
}