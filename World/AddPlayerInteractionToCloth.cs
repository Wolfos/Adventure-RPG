using System.Linq;
using UnityEngine;
using Utility;

public class AddPlayerInteractionToCloth : MonoBehaviour
{
    private void Start()
    {
        var cloth = GetComponent<Cloth>();
        var capsuleColliders = cloth.capsuleColliders.ToList();
        var player = SystemContainer.GetSystem<Player.Player>();
        capsuleColliders.Add(player.GetComponent<CapsuleCollider>());
        cloth.capsuleColliders = capsuleColliders.ToArray();
    }
}
