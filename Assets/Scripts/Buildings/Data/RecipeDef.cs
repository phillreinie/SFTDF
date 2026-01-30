using System;
using System.Collections.Generic;

[Serializable]
public class RecipeDef
{
    public List<ResourceAmount> inputs = new();
    public List<ResourceAmount> outputs = new();
}