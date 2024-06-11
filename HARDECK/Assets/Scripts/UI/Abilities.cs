using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AbilityObj
{
    public AbilityObj()
    {

    }

    public int referencedAbility;
    public int turnsRemaining;
}

public class AbilityDescriptor
{
    public AbilityDescriptor(string abNameInput, string abDescriptionInput, int abMaxTurnsRemaining)
    {
        abName = abNameInput;
        abDescription = abDescriptionInput;

        maxTurnsRemaining = abMaxTurnsRemaining;
    }

    public string abName;
    public string abDescription;

    public int maxTurnsRemaining;
}

public static class Abilities
{

    /*
     * CASE DICTIONARY
     * 0: ATTACK
     * 1: DIG IN
     * 2: BUILD STRUCTURE
    */
    public static AbilityDescriptor[] abilityDescs = new AbilityDescriptor[]
    {
        new AbilityDescriptor("Attack", "Fire the unit's primary weapon. Consumes an Action Pip", 1),
        new AbilityDescriptor("Move", "Attempt to manuver to the selected position. Consumes a Movement Pip", 1),
        new AbilityDescriptor("Dig In", "Unit braces at it's current position, confering a defense and accuracy boost for this turn. Consumes a Movement Pip", 1),
        new AbilityDescriptor("Build Structure", "Unit Is Stunned for two full turns, if Unit is alive at the end of the second turn, a structure is created at this location. Consumes an Action Pip", 3),

    };


    public static void UseAbility(int input, UnitAIBase castingUnit = null, UnitAIBase targetedUnit = null, Vector3Int targetedPosition = default(Vector3Int))
    {
        AbilityObj newAbility = new AbilityObj();
        newAbility.referencedAbility = input;

        switch (input)
        {
            case 0: //ATTACK
                castingUnit.Attack(targetedUnit);
                newAbility.turnsRemaining = 0;
                break;

            case 1: // MOVE
                castingUnit.Move(targetedPosition);
                break;

            case 2: //DIG IN
                castingUnit.f_dugIn = true;
                newAbility.turnsRemaining = 1;
                break;

            case 3: // BUILD STRUCTURE

                break;

            default:
                break;
        }

        castingUnit.activeAbilities.Add(newAbility);
    }

    public static void TickAbility(int input, UnitAIBase affectedUnit, Vector3Int affectedPosition = default(Vector3Int))
    {
        switch (input)
        {
            case 0: //ATTACK

                break;

            case 1: // MOVE

                break;

            case 2: //DIG IN

                break;


            case 3: // BUILD STRUCTURE

                break;

            default:
                break;
        }
    }


    public static void EndAbility(int input, UnitAIBase castingUnit = null, UnitAIBase targetedUnit = null, Vector3Int targetedPosition = default(Vector3Int))
    {
        switch (input)
        {
            case 0: //ATTACK

                break;

            case 1: //MOVE

                break;

            case 2: //DIG IN
                castingUnit.f_dugIn = false;

                break;


            case 3: // BUILD STRUCTURE

                break;

            default:
                break;
        }
    }
}
