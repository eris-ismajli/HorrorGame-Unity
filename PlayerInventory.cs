using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {

    public static PlayerInventory Instance { get; private set; }

    public event EventHandler<OnEquipEventArgs> OnEquip;

    public class OnEquipEventArgs : EventArgs {
        public EquipableObjectSO equippedObject;
    }

    private void Awake() {
        Instance = this;
    }

    private readonly List<EquipableObjectSO> equippedObjects = new List<EquipableObjectSO>();

    public void Equip(EquipableObjectSO equipableObject) {
        if (!equippedObjects.Contains(equipableObject)) {
            equippedObjects.Add(equipableObject);
            OnEquip?.Invoke(this, new OnEquipEventArgs {
                equippedObject = equipableObject
            });
        }
    }

    public void Unequip(EquipableObjectSO equipableObject) {
        if (equippedObjects.Contains(equipableObject)) {
            equippedObjects.Remove(equipableObject);
        }
    }

    public bool HasEquipableObject(EquipableObjectSO equipableObject) {
        return equippedObjects.Contains(equipableObject);
    }

    public bool HasAnyKey() {
        foreach (var equippedObj in equippedObjects) {
            if (equippedObj.isKey)
                return true;
        }

        return false;
    }

    public bool HasNoEquipableObject() {
        return equippedObjects.Count == 0;
    }
}
