using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointBlocker : MonoBehaviour
{
    public enum STATE
    {
        OPEN = 0,
        CLOSED = 1,
    }

    public enum POINT_STATE
    {
        INFERIOR = 0,
        INFERIOR_EQUAL = 1,
        EQUAL = 2,
        SUPERIOR = 3,
        SUPERIOR_EQUAL = 4,
    }

    public const string PLAYER_NAME = "Player";

    Utils.ORIENTATION _orientation = Utils.ORIENTATION.NONE;
    public Utils.ORIENTATION Orientation { get { return _orientation; } }

    STATE _state = STATE.OPEN;
    public POINT_STATE _pointState = POINT_STATE.INFERIOR;
    public STATE State { get { return _state; } }
    public GameObject closedGo = null;
    public GameObject openGo = null;

    public int doorValueIfLocked = 0;
    public bool takePointFromPlayer = false;

    public void Start()
    {
        if (closedGo.gameObject.activeSelf)
        {
            SetState(STATE.CLOSED);
        }
        else if (openGo.gameObject.activeSelf)
        {
            SetState(STATE.OPEN);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.parent != Player.Instance.gameObject.transform)
            return;
        switch (_state)
        {
            case STATE.CLOSED:
                if (CheckPointsToOpen(Player.Instance.GetPoint()))
                {
                    if(takePointFromPlayer)
                    {
                        if(Player.Instance.SpendPointsBlock(doorValueIfLocked))
                        {
                            SetState(STATE.OPEN);
                        }
                    }
                    else
                    {
                        SetState(STATE.OPEN);
                    }
                }
            break;
        }
    }

    public bool CheckPointsToOpen(int playerPoints)
    {
        switch (_pointState)
        {
            case POINT_STATE.INFERIOR:
                if(playerPoints < doorValueIfLocked)
                {
                    return true;
                }
                break;
            case POINT_STATE.INFERIOR_EQUAL:
                if (playerPoints <= doorValueIfLocked)
                {
                    return true;
                }
                break;
            case POINT_STATE.EQUAL:
                if (playerPoints == doorValueIfLocked)
                {
                    return true;
                }
                break;
            case POINT_STATE.SUPERIOR:
                if (playerPoints > doorValueIfLocked)
                {
                    return true;
                }
                break;
            case POINT_STATE.SUPERIOR_EQUAL:
                if (playerPoints >= doorValueIfLocked)
                {
                    return true;
                }
                break;
            default:
                return false;
        }
        return false;
    }

    public void SetState(STATE state)
    {
        if (closedGo) { closedGo.SetActive(false); }
        if (openGo) { openGo.SetActive(false); }
        _state = state;
        switch (_state)
        {
            case STATE.CLOSED:
                if (closedGo) { closedGo.SetActive(true); }
                break;
            case STATE.OPEN:
                if (openGo) { openGo.SetActive(true); }
                break;
        }
    }

    public void SetState(STATE state, int DoorValue)
    {
        if (closedGo) { closedGo.SetActive(false); }
        if (openGo) { openGo.SetActive(false); }
        _state = state;
        switch (_state)
        {
            case STATE.CLOSED:
                if (closedGo)
                {
                    closedGo.SetActive(true);
                    doorValueIfLocked = DoorValue;
                }
                break;
            case STATE.OPEN:
                if (openGo) { openGo.SetActive(true); }
                break;
        }
    }
}
