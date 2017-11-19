using UnityEngine;
using System.Collections;
using System;
using System.IO;
using Ionic.Zlib;

/// <summary>
/// 圆
/// </summary>
public struct Circle {
    public static readonly Circle zero = new Circle();
    public static readonly Circle one = new Circle(Vector2.zero, 1);

    public Vector2 center;
    public float radius;

    public Circle(Vector2 center, float radius) {
        this.center = center;
        this.radius = radius;
    }

    public static Circle operator +(Circle a, Vector2 d) {
        return new Circle(a.center + d, a.radius);
    }

    public static Circle operator +(Circle a, float d) {
        return new Circle(a.center, a.radius + d);
    }

    public static Circle operator *(Circle a, float d) {
        return new Circle(a.center, a.radius * d);
    }
    public static Circle operator /(Circle a, float d) {
        return new Circle(a.center, a.radius / d);
    }
    public static bool operator ==(Circle lhs, Circle rhs) {
        return lhs.radius == rhs.radius && lhs.center == rhs.center;
    }

    public static bool operator !=(Circle lhs, Circle rhs) {
        return lhs.radius != rhs.radius || lhs.center != rhs.center;
    }

    public override bool Equals(object other) {
        if(!(other is Circle)) {
            return false;
        }
        Circle round = (Circle)other;
        return this.center.Equals(round.center) && this.radius.Equals(round.radius);
    }

    public override int GetHashCode() {
        return (this.center.GetHashCode() ^ (this.radius.GetHashCode() << 2));
    }

    public override string ToString() {
        return "center:" + this.center + " radius:" + this.radius;
    }

    public bool Contains(Vector2 v) {
        return this.radius <= 0 ? false : (v - this.center).sqrMagnitude <= this.radius * this.radius;
    }
}

