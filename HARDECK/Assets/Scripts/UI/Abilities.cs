using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityObj
{
    public int referencedAbility;
    public int turnsRemaining;
}

public class AbilityDescriptor
{
    public AbilityDescriptor(string abNameInput, string abDescriptionInput)
    {
        abName = abNameInput;
        abDescription = abDescriptionInput;
    }

    public string abName;
    public string abDescription;

}

public static class Abilities
{

    /*
     * CASE DICTIONARY
     * 0: ATTACK
     * 1: DIG IN
     * 
    */
    public static AbilityDescriptor[] abilities = new AbilityDescriptor[]
    {
        new AbilityDescriptor("Attack", "Fire the unit's primary weapon. Consumes an Action Pip"),
        new AbilityDescriptor("Dig In", "Unit braces at it's current position, confering a defense and accuracy boost for this turn. Consumes a Movement Pip"),

    };


    public static void UseAbility(int input, UnitAIBase castingUnit = null, UnitAIBase targetedUnit = null)
    {
        AbilityObj newAbility = new AbilityObj();
        newAbility.referencedAbility = input;

        switch (input)
        {
            case 0: //ATTACK
                castingUnit.Attack(targetedUnit);
                newAbility.turnsRemaining = 0;
                break;

            case 1: //DIG IN
                castingUnit.f_dugIn = true;
                newAbility.turnsRemaining = 1;
                break;

            default:
                break;
        }

        castingUnit.activeAbilities.Add(newAbility);
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
