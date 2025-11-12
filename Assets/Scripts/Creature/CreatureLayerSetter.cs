using UnityEngine;

public class CreatureLayerSetter : MonoBehaviour
{
    private void Start()
    {
        CreatureBase creature = GetComponent<CreatureBase>();
        if (creature != null)
        {
            if (creature.isPlayerCreature)
            {
                gameObject.layer = LayerMask.NameToLayer("PlayerCreature");
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("EnemyCreature");
            }
        }
    }
}