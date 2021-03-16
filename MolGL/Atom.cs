using System;
namespace MolGL
{
    public class Atom : IComparable
    {
        public int CompareTo(object obj)
        {
            if(obj is Atom)
            {
                Atom otherAtom = (Atom) obj;
                return this.Num.CompareTo(otherAtom.Num);
            }
            else
                throw new ArgumentException("Object is not a Atom");
        }

        public Atom()
        {
            numNext = new ushort[4];
        }
        public void setCoord(float xi, float yi, float zi)
        {
            X = xi;
            Y = yi;
            Z = zi;
        }

        public void setTransCoord(float tx, float ty, float tz)
        {
            TX += tx;
            TY += ty;
            TZ += tz;
        }

        public void setDispCoord(float xc, float yc, float zc)
        {
            TX = X - xc;
            TY = Y - yc;
            TZ = Z - zc;
        }

        public void setNum(ushort number)
        {
            Num = number;
            numNext[0] = number;
            numNext[1] = number;
            numNext[2] = number;
            numNext[3] = number;
        }

        public void setNextAtom(ushort num, ushort next)
        {
            if (num > 3) {
                //System.out.println("there is not such number");
            } else
                numNext[num] = next;
        }
        public ushort getNextAtom(byte num)
        {
            if (num > 3) {
                //System.out.println("there is not such number");
                return 0;
            }else
                return numNext[num];
        }

   /*     public void setColor(Color c)
        {
            R = (c % 0x100) / 255;
            G = ((c / 0x100) % 0x100) / 255;
            B = (c / 0x10000) / 255;
        }
        */
        public void setX(float coord) { X = coord; }
        public void setY(float coord) { Y = coord; }
        public void setZ(float coord) { Z = coord; }

        public void setTX(float trans) { TX += trans; }
        public void setTY(float trans) { TY += trans; }
        public void setTZ(float trans) { TZ += trans; }

        public void wrtTX(float trans) { TX = trans; }
        public void wrtTY(float trans) { TY = trans; }
        public void wrtTZ(float trans) { TZ = trans; }
      
        public void setName(string name) { Name = name; }

        //public void setNum(ushort number) { itsNum = number; }

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        public float TX { get; private set; }
        public float TY { get; private set; }
        public float TZ { get; private set; }

        public ushort Num { get; private set; }
        private ushort[] numNext;
        public string Name { get; private set; }
        public float R { get; private set; }
        public float G { get; private set; }
        public float B { get; private set; }
    }
}
