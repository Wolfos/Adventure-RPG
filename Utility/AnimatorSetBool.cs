using UnityEngine;

namespace Utility
{
	public class AnimatorSetBool : StateMachineBehaviour
	{
		public bool onUpdate;
		[SerializeField] private string variableName;
		[SerializeField] private bool setting;


		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			base.OnStateEnter(animator, stateInfo, layerIndex);
			animator.SetBool(variableName, setting);
		}

		public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			base.OnStateUpdate(animator, stateInfo, layerIndex);
			if (onUpdate)
			{
				animator.SetBool(variableName, setting);
			}
		}
	}
}