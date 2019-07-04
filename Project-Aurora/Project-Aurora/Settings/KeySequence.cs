using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Aurora.Settings {
    /// <summary>
    /// The type of the KeySequence
    /// </summary>
    public enum KeySequenceType
    {
        /// <summary>
        /// Sequence uses an array of DeviceKeys keys
        /// </summary>
        Sequence,
        /// <summary>
        /// Sequence uses a freeform region
        /// </summary>
        FreeForm
    }

    /// <summary>
    /// A class representing a series of DeviceKeys keys or a freeform region
    /// </summary>
    public class KeySequence : ICloneable, INotifyPropertyChanged
    {
        // Write-only property to set the original keys as a List, since it won't work if trying to set the observable collection as a list directly.
        #pragma warning Should be removed in a future (breaking) version.
        [JsonProperty("keys"), Obsolete("For Newtonsoft.Json use only.")]
        private List<Devices.DeviceKeys> RawList { set => Keys = new ObservableCollection<Devices.DeviceKeys>(value); }

        /// <summary>
        /// An array of DeviceKeys keys to be used with KeySequenceType.Sequence type.
        /// </summary>
        [JsonProperty("keyCollection")]
        public ObservableCollection<Devices.DeviceKeys> Keys { get => keys; set => SetAndNotify(ref keys, value); }
        private ObservableCollection<Devices.DeviceKeys> keys;

        /// <summary>
        /// The type of this KeySequence instance.
        /// </summary>
        [JsonProperty("type")]
        public KeySequenceType Type { get => type; set => SetAndNotify(ref type, value); }
        private KeySequenceType type;

        /// <summary>
        /// The Freeform object to be used with KeySequenceType.FreeForm type
        /// </summary>
        [JsonProperty("freeform")]
        public FreeFormObject Freeform { get => freeform; set => SetAndNotify(ref freeform, value); }
        private FreeFormObject freeform;

        public KeySequence()
        {
            Keys = new ObservableCollection<Devices.DeviceKeys>();
            Type = KeySequenceType.Sequence;
            Freeform = new FreeFormObject();
        }

        public KeySequence(KeySequence otherKeysequence)
        {
            Keys = new ObservableCollection<Devices.DeviceKeys>(otherKeysequence.Keys);
            Type = otherKeysequence.Type;
            Freeform = otherKeysequence.Freeform;
        }

        public KeySequence(FreeFormObject freeform)
        {
            Keys = new ObservableCollection<Devices.DeviceKeys>();
            Type = KeySequenceType.FreeForm;
            Freeform = freeform;
        }

        public KeySequence(IEnumerable<Devices.DeviceKeys> keys)
        {
            Keys = new ObservableCollection<Devices.DeviceKeys>(keys);
            Type = KeySequenceType.Sequence;
            Freeform = new FreeFormObject();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public RectangleF GetAffectedRegion()
        {
            switch (Type)
            {
                case KeySequenceType.FreeForm:
                    return new RectangleF((this.Freeform.X + Effects.grid_baseline_x) * Effects.editor_to_canvas_width, (this.Freeform.Y + Effects.grid_baseline_y) * Effects.editor_to_canvas_height, this.Freeform.Width * Effects.editor_to_canvas_width, this.Freeform.Height * Effects.editor_to_canvas_height);
                default:

                    float left = 0.0f;
                    float top = left;
                    float right = top;
                    float bottom = right;

                    foreach(Devices.DeviceKeys key in this.Keys)
                    {
                        BitmapRectangle keyMapping = Effects.GetBitmappingFromDeviceKey(key);

                        if(left == top && top == right && right == bottom && bottom == 0.0f)
                        {
                            left = keyMapping.Left;
                            top = keyMapping.Top;
                            right = keyMapping.Right;
                            bottom = keyMapping.Bottom;
                        }
                        else
                        {
                            if (keyMapping.Left < left)
                                left = keyMapping.Left;
                            if (keyMapping.Top < top)
                                top = keyMapping.Top;
                            if (keyMapping.Right > right)
                                right = keyMapping.Right;
                            if (keyMapping.Bottom > bottom)
                                bottom = keyMapping.Bottom;
                        }
                    }

                    return new RectangleF(left, top, (right - left), (bottom - top));
            }

        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KeySequence)obj);
        }

        public bool Equals(KeySequence p)
        {
            if (ReferenceEquals(null, p)) return false;
            if (ReferenceEquals(this, p)) return true;

            return (new HashSet<Devices.DeviceKeys>(Keys).SetEquals(p.Keys)) &&
                Type == p.Type &&
                Freeform.ValuesEqual(p.Freeform);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Keys.GetHashCode();
                hash = hash * 23 + Type.GetHashCode();
                hash = hash * 23 + Freeform.GetHashCode();
                return hash;
            }
        }

        public object Clone()
        {
            return new KeySequence(this);
        }

        private void SetAndNotify<T>(ref T field, T value, [CallerMemberName] string prop = null) {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
