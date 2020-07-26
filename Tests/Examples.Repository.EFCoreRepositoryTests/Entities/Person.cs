using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Examples.Repository.EFCoreRepositoryTests.Entities
{
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class Person : IEquatable<Person>
    {
        public Person()
        {
        }

        public Person(byte age, string firstName, string lastName)
        {
            Age = age;
            FirstName = firstName;
            LastName = lastName;
        }

        public bool Equals(Person other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id && Age == other.Age &&
                   FirstName == other.FirstName && 
                   LastName == other.LastName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Person)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Age, FirstName, LastName);
        }

        [Key]
        public int Id { get; private set; }

        public byte Age { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}