using System;
using System.Collections;
using Combat;
using Data;
using UnityEngine;
using UnityEngine.AI;
using Utility;

namespace Character
{
	public enum NPCDemeanor
	{
		Friendly, Neutral, Hostile
	}

	public enum NPCRoutine
	{
		Idle, Wandering, Combat
	}
	
	public class NPC : CharacterBase
	{
		[Header("NPC settings")][SerializeField] private NPCDemeanor demeanor;
		[SerializeField] private NPCRoutine defaultRoutine;
		[SerializeField] private NavMeshAgent agent;

		[SerializeField] private int[] startingInventory;
		
		[SerializeField] private float meleeAttackRange = 1;
		[SerializeField] private float maxPursueDistance = 20;
		[SerializeField] private float startChaseDistance = 10;
		
		[HideInInspector] public Bounds bounds;
		[HideInInspector] public bool respawn = false;

		private NPCRoutine currentRoutine;

		public Action OnDeath;

		private Player.Player player;

		private new void OnEnable()
		{
			if(agent.enabled) Debug.LogError("NavmeshAgent was enabled by default. Due to a bug in Unity, it should start disabled");

			if (respawn)
			{
				Debug.Log("Respawning");
				animator.SetTrigger("Spawn");
				StartCoroutine(SpawnAnimation());
			}
			else
			{
				ActivateRoutine(defaultRoutine, true);
			}

			onDamaged += OnDamaged;
			
			base.OnEnable();
		}

		private IEnumerator SpawnAnimation()
		{
			yield return new WaitForSeconds(1);
			
			ActivateRoutine(defaultRoutine, true);
		}

		private void Start()
		{
			data.position = transform.position;
			data.rotation = transform.rotation;

			for (int i = 0; i < startingInventory.Length; i++)
			{
				inventory.AddItem(startingInventory[i]);
				inventory.items[i].Equipped = true;
			}
			equipment.CheckEquipment();

			player = SystemContainer.GetSystem<Player.Player>();
			base.Start();
		}

		public void UpdateData(CharacterData data)
		{
			this.data = data;
			SetHealth(data.health);
			transform.position = data.position;
			transform.rotation = data.rotation;
			CharacterPool.Register(data.characterId, this);
			ActivateRoutine(data.routine, true, true);
		}

		protected override void DeathAnimationStarted()
		{
			StopAllCoroutines();
			agent.enabled = false;
		}

		protected override void DeathAnimationFinished()
		{
			respawn = true;
			gameObject.SetActive(false);
		}
		
		private void ActivateRoutine(NPCRoutine routine, bool delayed = false, bool proceed = false)
		{
			StopAllCoroutines();

			currentRoutine = routine;
			data.routine = routine;
			switch (routine)
			{
				case NPCRoutine.Idle:
					break;
				case NPCRoutine.Wandering:
					StartCoroutine(WanderingRoutine(delayed, proceed));
					break;
				case NPCRoutine.Combat:
					StartCoroutine(CombatRoutine(delayed, proceed));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(routine), routine, null);
			}
		}

		private void Update()
		{
			animator.SetFloat("Speed", agent.velocity.magnitude);

			data.position = transform.position;
			data.rotation = transform.rotation;
			data.velocity = agent.velocity;

			base.Update();
		}

		private void OnDamaged(Damage damage)
		{
			if (!string.IsNullOrEmpty(damage.source))
			{
				data.currentTarget = damage.source;
				if (currentRoutine != NPCRoutine.Combat)
				{
					ActivateRoutine(NPCRoutine.Combat);
				}
			}

			if (data.health - damage.amount <= 0) // Dying
			{
				OnDeath?.Invoke();
				CharacterPool.GetCharacter(damage.source)?.Killed(gameObject.name);
			}
			
		}

		private CharacterBase GetTarget()
		{
			return CharacterPool.GetCharacter(data.currentTarget);
		}

		private IEnumerator CombatRoutine(bool delayed = false, bool proceed = false)
		{
			if (delayed)
			{
				yield return null;
				yield return null;
			}

			if (!agent.enabled) agent.enabled = true;
			
			while (true)
			{
				if (proceed) 
				{
					agent.velocity = data.velocity;
					yield return null;
				}

				while (GetTarget() == null) yield return null;

				Vector3 targetPos = GetTarget().transform.position;
				Vector3 pos = transform.position;
				
				data.destination = targetPos;
				agent.destination = targetPos;
				
				float distance = Vector3.Distance(targetPos, pos);
				var targetDir = targetPos - pos;
				targetDir.y = 0;
				float angle = Vector3.Angle(targetDir, graphic.forward);

				if (distance > maxPursueDistance)
				{
					ActivateRoutine(defaultRoutine);
				}
				else if (distance < meleeAttackRange && Mathf.Abs(angle) < 10)
				{
					data.destination = transform.position;
					agent.destination = data.destination;
					StartCoroutine(AttackRoutine());
					yield return new WaitForSeconds(1);
				}

				proceed = false;
				
				yield return null;
			}
		}

		private IEnumerator AttackRoutine()
		{
			animator.SetTrigger("Telegraph");
			yield return null;
			var clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
			var length = clip.length;
			yield return new WaitForSeconds(length - 0.1f);
			Attack();
		}
		
		private IEnumerator WanderingRoutine(bool delayed = false, bool proceed = false)
		{
			if (delayed)
			{
				yield return null;
				yield return null;
			}

			if (!agent.enabled) agent.enabled = true;
			
			while (true)
			{
				if (proceed) agent.velocity = data.velocity;
				else data.destination = bounds.RandomPos();

				agent.destination = data.destination;

				if (proceed) yield return null;

				while (agent.hasPath)
				{
					if (demeanor == NPCDemeanor.Hostile)
					{
						if (Vector3.SqrMagnitude(transform.position - player.transform.position) < startChaseDistance * startChaseDistance)
						{
							data.currentTarget = player.data.characterId;
							ActivateRoutine(NPCRoutine.Combat);
						}
					}
					if (Mathf.Abs(agent.velocity.magnitude) < 0.01f) // Stuck
					{
						break;
					}
					yield return null;
				}

				proceed = false;
				
				yield return null;
			}
		}

		private void OnDisable()
		{
			onDamaged -= OnDamaged;
		}
	}
}