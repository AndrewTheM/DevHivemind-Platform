﻿namespace BlogPlatform.Verifications.Domain.Abstract;

public abstract class ValueObject
{
    public static bool operator ==(ValueObject left, ValueObject right)
    {
        return EqualOperator(left, right);
    }

    public static bool operator !=(ValueObject left, ValueObject right)
    {
        return NotEqualOperator(left, right);
    }

    public override bool Equals(object obj)
    {
        if (obj is null || obj.GetType() != this.GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return this.GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => (x is not null) ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (left is null ^ right is null)
        {
            return false;
        }

        return left is null || left.Equals(right);
    }

    protected static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !EqualOperator(left, right);
    }

    protected abstract IEnumerable<object> GetEqualityComponents();
}
