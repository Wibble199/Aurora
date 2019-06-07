using System;

namespace Aurora.Settings.Overrides.Logic {

    /// <summary>
    /// A class that represents an hour and minute of an arbitrary day.
    /// </summary>
    public class Time : IEquatable<Time> {

        private int hour;
        private int minute;


        /// <summary>
        /// Creates a new <see cref="Time"/> with hour and minute both set to zero (midnight).
        /// </summary>
        public Time() : this(0, 0) { }

        /// <summary>
        /// Creates a new <see cref="Time"/> with the given hour and minute.
        /// </summary>
        public Time(int hour, int minute) {
            this.hour = hour % 24;
            this.minute = minute % 60;
        }


        /// <summary>
        /// Gets a <see cref="Time"/> object representing the current time.
        /// </summary>
        public static Time Now => new Time(DateTime.Now.Hour, DateTime.Now.Minute);

        /// <summary>
        /// Gets or sets this <see cref="Time"/>'s hour. This will be constrained to a max of 23.
        /// </summary>
        public int Hour { get => hour; set => hour = Math.Max(Math.Min(value, 23), 0); }

        /// <summary>
        /// Gets or sets this <see cref="Time"/>'s minute. This will be constrained to a max of 59.
        /// </summary>
        public int Minute { get => minute; set => minute = Math.Max(Math.Min(value, 59), 0); }


        /// <summary>
        /// Determines if two <see cref="Time"/> instances are equal (have the same hour and minute).
        /// </summary>
        public static bool operator ==(Time lhs, Time rhs) => Equals(lhs, rhs);
        /// <summary>
        /// Determines if two <see cref="Time"/> instances are not equal (have differing hour or minute).
        /// </summary>
        public static bool operator !=(Time lhs, Time rhs) => !Equals(lhs, rhs);

        /// <summary>
        /// Determines if one <see cref="Time"/> instance occurs earlier than another (assuming they were to happen on the same day).
        /// </summary>
        public static bool operator <(Time lhs, Time rhs) =>
            lhs.Hour < rhs.Hour || (lhs.Hour == rhs.Hour && lhs.Minute < rhs.Minute);

        /// <summary>
        /// Determines if one <see cref="Time"/> instance occurs later than another (assuming they were to happen on the same day).
        /// </summary>
        public static bool operator >(Time lhs, Time rhs) =>
            lhs.Hour > rhs.Hour || (lhs.Hour == rhs.Hour && lhs.Minute > rhs.Minute);

        /// <summary>
        /// Determines if one <see cref="Time"/> instance occurs earlier or at the same time as another (assuming they were to happen on the same day).
        /// </summary>
        public static bool operator <=(Time lhs, Time rhs) => lhs < rhs || lhs == rhs;

        /// <summary>
        /// Determines if one <see cref="Time"/> instance occurs later or at the same time as another (assuming they were to happen on the same day).
        /// </summary>
        public static bool operator >=(Time lhs, Time rhs) => lhs > rhs || lhs == rhs;

        /// <summary>
        /// Determines if two <see cref="Time"/> instances are equal (have the same hour and minute).
        /// </summary>
        public override bool Equals(object obj) => Equals(obj as Time);
        /// <summary>
        /// Determines if two <see cref="Time"/> instances are equal (have the same hour and minute).
        /// </summary>
        public bool Equals(Time other) => other != null && Hour == other.Hour && Minute == other.Minute;

        public override int GetHashCode() {
            var hashCode = 1676104416;
            hashCode = hashCode * -1521134295 + hour.GetHashCode();
            hashCode = hashCode * -1521134295 + minute.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Determins if the given time is between the given start and end times. Note that this will "wrap" around midnight. For example,
        /// if the start time was 23:00 and the end was 04:00; and the given value was 01:00, this is classed as being between.
        /// The comparison is inclusive: 14:00 is classed between 14:00 and 15:00.
        /// </summary>
        public static bool IsBetween(Time value, Time start, Time end) => start > end
            ? value >= start || value <= end
            : value >= start && value <= end;

        /// <summary>
        /// Formats this time as a 24-hour time.
        /// </summary>
        public override string ToString() => string.Format("{0:D2}:{1:D2}", Hour, Minute);

        /// <summary>
        /// Creates a new time from this time.
        /// </summary>
        public Time Clone() => new Time(Hour, Minute);
    }
}
