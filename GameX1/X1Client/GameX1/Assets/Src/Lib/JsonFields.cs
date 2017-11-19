using System;

[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public class JsonFields: Attribute {
    public string[] fields;
}
