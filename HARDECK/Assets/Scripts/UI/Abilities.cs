using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public AbilityDescriptor(string abNameInput, string abDescriptionInput, int abMaxTurnsRemaining, Color abColor, string abCostMessage, string abParametersMessage)
    {
        abName = abNameInput;
        abDescription = abDescriptionInput;

        maxTurnsRemaining = abMaxTurnsRemaining;

        color = abColor;

        costMessage = abCostMessage;

        parametersMessage = abParametersMessage;
    }

    public string abName;
    public string abDescription;

    public int maxTurnsRemaining;

    public Color color;

    public string costMessage;

    public string parametersMessage;
}

public static class Abilities
{

    /*
     * CASE DICTIONARY
     * 0: ATTACK
     * 1: MOVE
     * 2: DIG IN
     * 3: BUILD STRUCTURE
     * 4: FIRE MISSILE
    */
    public static AbilityDescriptor[] abilityDescs = new AbilityDescriptor[]
    {
        new AbilityDescriptor("Attack", "Fire the unit's primary weapon", 1, new Color(1,0,0), "Requires an Action Pip", "Select an enemy Unit"),
        new AbilityDescriptor("Move", "Attempt to manuver to the selected position", 1, new Color(1,0.75f,0), "Requires a Movement Pip", "Select a location to move to, within the units range"),
        new AbilityDescriptor("Dig In", "Unit braces at it's current position, confering a defense and accuracy boost for this turn", 1, new Color(0.25f, 0.25f, 0.25f), "Requires a Movement Pip", "Huh?"),
        new AbilityDescriptor("Build Structure", "Unit Is Stunned for two full turns, if Unit is alive at the end of the second turn, a structure is created at this location", 3, new Color(0, 0.25f, 1), "Requires an Action Pip and a Movement Pip", "Location Invalid"),
        new AbilityDescriptor("Fire Missile", "Fire 3x3 Splash damage Missile, dealing 5 damage to effected units", 1, new Color(1, 0.25f, 0.25f), "Requires an Action Pip", "Select a location or enemy within range"),

    };


    public static void UseAbility(int input, UnitAIBase castingUnit = null, UnitAIBase targetedUnit = null, Vector3Int targetedPosition = default(Vector3Int))
    {
        AbilityObj newAbility = new AbilityObj();
        AbilityDescriptor desc = abilityDescs[input];
        newAbility.referencedAbility = input;
        newAbility.turnsRemaining = desc.maxTurnsRemaining;

        switch (input)
        {
            case 0: //ATTACK
                castingUnit.actionPips--;
                castingUnit.Attack(targetedUnit);
                break;

            case 1: // MOVE
                castingUnit.movementPips--;
                castingUnit.Move(targetedPosition);
                break;

            case 2: //DIG IN
                castingUnit.movementPips--;
                castingUnit.f_dugIn = true;
                break;

            case 3: // BUILD STRUCTURE
                castingUnit.actionPips--;
                castingUnit.movementPips--;
                break;

            case 4: // FIRE MISSILE
                castingUnit.actionPips--;

                Vector3Int landPos;
                if (targetedUnit != null)
                {
                    landPos = targetedUnit.currentPos;
                }
                else
                {
                    landPos = targetedPosition;
                }

                LayerMask mask = LayerMask.GetMask("Unit");
                Collider[] nearbyUnits = Physics.OverlapBox(landPos, new Vector3(1,1,1));

                foreach(Collider c in nearbyUnits)
                {
                    if (c.GetComponent<UnitAIBase>())
                    {
                        c.GetComponent<UnitAIBase>().TakeDamage(6, landPos);
                    }
                }

                break;

            default:
                break;
        }

        castingUnit.activeAbilities.Add(newAbility);
        GameManager.instance.UpdateVisableFOW();

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
                affectedUnit.actionPips = 0;
                affectedUnit.movementPips = 0;
                break;

            case 4: // FIRE MISSILE

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

                GameObject sObj = GameObject.Find("Structures");
                GameObject structurePrefab = Resources.Load("Structures/StructureTest") as GameObject;


                GameObject obj = GameManager.Instantiate(structurePrefab, new Vector3(castingUnit.currentPos.x, castingUnit.currentPos.y, castingUnit.currentPos.z), Quaternion.identity);

                Structure sLogic = obj.GetComponent<Structure>();

                // Position
                sLogic.tilemapPos = new Vector3Int(castingUnit.currentPos.x, castingUnit.currentPos.y, castingUnit.currentPos.z);

                // WidthHeight
                sLogic.WidthHeight = new Vector2Int(1, 1);

                // Owner UD
                sLogic.ownerAlliance = castingUnit.alliance;

                // Provides
                sLogic.provideGround = true;
                sLogic.provideHeavy = false;
                sLogic.provideAir = false;

                // Is Player HQ
                sLogic.playerHq = false;

                // Set In-World position and proper parenting
                obj.transform.position = sLogic.tilemapPos;
                obj.transform.SetParent(sObj.transform);

                GameManager.instance.commanders[sLogic.ownerAlliance].ownedStructures.Add(sLogic);
                GameManager.instance.UpdateVisableFOW();
                break;

            case 4: // FIRE MISSILE

                break;

            default:
                break;
        }
    }

    public static bool CheckAbilityCost(UnitAIBase uInput, int input)
    {
        bool result = false;

        if (uInput != null)
        {
            switch (input)
            {
                case 0: //ATTACK

                    if (uInput.actionPips > 0)
                    {
                        result = true;
                    }
                    break;

                case 1: //MOVE

                    if (uInput.movementPips > 0)
                    {
                        result = true;
                    }
                    break;

                case 2: //DIG IN

                    if (uInput.movementPips > 0)
                    {
                        result = true;
                    }
                    break;

                case 3: // BUILD STRUCTURE

                    if (uInput.actionPips > 0 && uInput.movementPips > 0)
                    {
                        result = true;
                    }
                    break;

                case 4: // FIRE MISSILE

                    if (uInput.actionPips > 0)
                    {
                        result = true;
                    }
                    break;

                default:
                    break;
            }
        }
        return result;
    }

    public static bool CheckAbilityParameters(UnitAIBase uInput, int input)
    {
        bool result = false;

        switch (input)
        {
            case 0: //ATTACK

                if (GameManager.instance.selectedUnit_Enemy != null)
                {
                    result = true;
                }
                break;

            case 1: //MOVE

                Vector3Int p = GameManager.instance.selectedPosition;
                if (MapBuilder.instance.Tiles[p.x, p.y, p.z] != null)
                {
                    if (GameManager.instance.selectedPosition != new Vector3Int(-1, -1, -1) && MapBuilder.instance.Flowfields[uInput.currentPos.x, uInput.currentPos.y, uInput.currentPos.z].tiles[p.x, p.y, p.z].pathCost <= uInput.moveSpeed)
                    {
                        result = true;
                    }
                }
                break;

            case 2:

                result = true;

                break;

            case 3: // BUILD STRUCTURE

                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        if (MapBuilder.instance.Tiles[uInput.currentPos.x + x, uInput.currentPos.y, uInput.currentPos.z + y] == null || MapBuilder.instance.Tiles[uInput.currentPos.x + x, uInput.currentPos.y, uInput.currentPos.z + y].isRamp)
                        {
                            return false;
                        }
                    }
                }
                result = true;

                break;

            case 4: // FIRE MISSILE

                if (GameManager.instance.selectedPosition == null && GameManager.instance.selectedUnit_Enemy == null)
                {
                    return false;
                }
                if (GameManager.instance.selectedPosition != null)
                {
                    if (Vector3.Distance(GameManager.instance.selectedPosition, uInput.currentPos) > uInput.range)
                    {
                        return false;
                    }
                }
                return true;
                break;

            default:
                break;
        }

        return result;
    }
}
