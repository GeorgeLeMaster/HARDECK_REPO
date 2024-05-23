using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityObj
{
    public int referencedAbility;
    public int turnsRemaining;
}

public class Abilities : MonoBehaviour
{

    /*
     * CASE DICTIONARY
     * 0: ATTACK
     * 1: DIG IN
     * 
    */



    public static void UseAbility(int input, UnitAIBase castingUnit = null, UnitAIBase targetedUnit = null)
    {
        switch (input)
        {
            case 0: //ATTACK
                castingUnit.Attack(targetedUnit);

                break;

            case 1: //DIG IN
                castingUnit.f_dugIn = true;

                break;

            default:
                break;
        }
    }

    public static void TickAbility(int input, UnitAIBase affectedUnit, Vector3? affectedPosition)
    {
        switch (input)
        {
            case 0: //ATTACK

                break;

            case 1: //DIG IN

                break;

            default:
                break;
        }
    }


    public static void EndAbility(int input, UnitAIBase castingUnit = null, UnitAIBase targetedUnit = null)
    {
        switch (input)
        {
            case 0: //ATTACK

                break;

            case 1: //DIG IN
                castingUnit.f_dugIn = false;

                break;

            default:
                break;
        }
    }
}
