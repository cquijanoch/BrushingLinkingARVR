using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public static class Extensions
{
    public static void SetActiveRecursivelyExt(this GameObject obj, bool state)
    {
        obj.SetActive(state);
        foreach (Transform child in obj.transform)
        {
            SetActiveRecursivelyExt(child.gameObject, state);
        }
    }

    public static Stack<T> Clone<T>(this Stack<T> stack)
    {
        Contract.Requires(stack != null);
        return new Stack<T>(new Stack<T>(stack));
    }
}
