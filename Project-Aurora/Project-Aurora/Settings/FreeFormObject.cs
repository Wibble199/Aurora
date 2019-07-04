namespace Aurora.Settings
{
    /// <summary>
    /// The type of the FreeForm region.
    /// </summary>
    public enum FreeFormType
    {
        Line,
        Rectangle,
        Circle,
        RectangleFilled,
        CircleFilled
    }

    /// <summary>
    /// A delegate for a changed value
    /// </summary>
    /// <param name="newobject">The current instance of FreeFormObject</param>
    public delegate void ValuesChangedEventHandler(FreeFormObject newobject);

    /// <summary>
    /// A class representing a region within a bitmap.
    /// </summary>
    public class FreeFormObject
    {
        FreeFormType _type;
        /// <summary>
        /// Get/Set the type of this region.
        /// </summary>
        public FreeFormType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                ValuesChanged?.Invoke(this);
            }
        }

        float _x;
        /// <summary>
        /// Get/Set the X coordinate for this region.
        /// </summary>
        public float X
        {
            get { return _x; }
            set
            {
                _x = value;
                ValuesChanged?.Invoke(this);
            }
        }

        float _y;
        /// <summary>
        /// Get/Set the Y coordinate for this region.
        /// </summary>
        public float Y
        {
            get { return _y; }
            set
            {
                _y = value;
                ValuesChanged?.Invoke(this);
            }
        }

        float _width;
        /// <summary>
        /// Get/Set the Width of this region.
        /// </summary>
        public float Width
        {
            get { return _width; }
            set
            {
                _width = value;
                ValuesChanged?.Invoke(this);
            }
        }

        float _height;
        /// <summary>
        /// Get/Set the Height of this region.
        /// </summary>
        public float Height
        {
            get { return _height; }
            set
            {
                _height = value;
                ValuesChanged?.Invoke(this);
            }
        }

        float _angle;
        /// <summary>
        /// Get/Set the rotation angle of this region.
        /// </summary>
        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                ValuesChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Event for when any value of this FreeFormObject changes.
        /// </summary>
        public event ValuesChangedEventHandler ValuesChanged;

        /// <summary>
        /// Creates a default instance of the FreeFormObject
        /// </summary>
        public FreeFormObject()
        {
            _type = FreeFormType.Rectangle;
            _x = 0;
            _y = 0;
            _width = 30;
            _height = 30;
            _angle = 0.0f;
        }

        /// <summary>
        /// Creates an instance of the FreeFormObject with specified parameters.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        /// <param name="width">The Width</param>
        /// <param name="height">The Height</param>
        /// <param name="angle">The rotation angle</param>
        public FreeFormObject(float x, float y, float width = 30.0f, float height = 30.0f, float angle = 0.0f)
        {
            _type = FreeFormType.Rectangle;
            _x = x;
            _y = y;
            _width = width;
            _height = height;
            _angle = angle;
        }

        /// <summary>
        /// An equals function, compares this instance of FreeFormObject to another instance of FreeFormObject and returns whether or not they contain equal values.
        /// </summary>
        /// <param name="p">An instance of FreeFormObject to be compared</param>
        /// <returns>A boolean value representing equality</returns>
        public bool ValuesEqual(FreeFormObject p)
        {
            return _type == p._type &&
                _x == p._x &&
                _y == p._y &&
                _width == p._width &&
                _height == p._height &&
                _angle == p._angle;
        }


        /* There used to be an override for this class's hashcode method here. It has now been removed.
         * This was what was causing the to be collisions when two freeform objects were overlapping to be treated as equal.
         * The hashcode was based purely off the mutable elements of the class, and therefore if two classes had identical elements
         * they were treated equal. This is fine until you have multiple freeforms active at a time (e.g. some of the more complex layers
         * such as ETS2 blinker layer, or when the user has two from using the overrides system with the affect keys). When you have two
         * freeform objects with the same values, there is weird behaviour if the user decides to not use the freeform and they both disappear.
         * Additionally, this means the freeform object cannot be used in a dictionary as the dictionary uses the hascode when creating the keys.
         * There is plenty of discussion to be had online as to why you should not use mutable fields to generate hashcodes. Such as this from
         * StackOverflow:
         * "If you have a mutable object, there isn't much point in overriding the GetHashCode method, as you can't really use it. It's used
         * for example by the Dictionary and HashSet collections to place each item in a bucket. If you change the object while it's used as
         * a key in the collection, the hash code no longer matches the bucket that the object is in, so the collection doesn't work properly
         * and you may never find the object again." */
    }
}
