using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAttribute
{
    public int BaseValue {
        get { return _baseValue; }
    }
    int _baseValue;

    public float BaseMultiplier {
        get { return _baseMultiplier; }
    }
    float _baseMultiplier;

    public BaseAttribute(int baseValue, float baseMultiplier) {
        this._baseValue = baseValue;
        this._baseMultiplier = baseMultiplier;
    }

    public BaseAttribute(int baseValue) {
        this._baseValue = baseValue;
        this._baseMultiplier = 0f;
    }
}
