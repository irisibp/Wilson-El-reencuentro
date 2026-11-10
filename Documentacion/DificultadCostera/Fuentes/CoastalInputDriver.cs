using System.Reflection;
using UnityEngine;

// Isolated Play Mode harness: exercises the real controller with deterministic input.
[DefaultExecutionOrder(100)]
public class CoastalInputDriver : MonoBehaviour
{
    public WilsonController player;
    public float horizontal;
    public bool jump;
    static readonly FieldInfo movement=typeof(WilsonController).GetField("movimiento",BindingFlags.Instance|BindingFlags.NonPublic);
    static readonly FieldInfo jumpRequest=typeof(WilsonController).GetField("solicitarSalto",BindingFlags.Instance|BindingFlags.NonPublic);
    private void FixedUpdate()
    {
        if(player==null || !player.isActiveAndEnabled)return;
        movement.SetValue(player,horizontal);
        if(jump){jumpRequest.SetValue(player,true);jump=false;}
        player.SendMessage("FixedUpdate");
    }
}
