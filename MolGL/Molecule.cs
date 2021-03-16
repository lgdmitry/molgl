using System;
using System.Windows.Forms;
using System.Collections;
using System.IO;

using Microsoft.VisualBasic;
using System.Text;


using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform.Windows;

namespace MolGL
{
    public class Molecule
    {   	    

        public Molecule(ushort amount)
	    {
            createMol(amount);
	    }

        private void createMol(ushort amount)
        {
            M = amount;
	        Radius = 0.18F;
            Slices = 20;
	        itsAtom = new Atom[amount];
            for (ushort i=0; i < itsAtom.Length; i++)
                itsAtom[i] = new Atom();
        }

	    public void trX(float trans,short num)
	    {
            if (num < 0)
                foreach (Atom atom in itsAtom)
                    atom.setTX(trans);
            else
                itsAtom[num].setTX(trans);
	        nextCenterCoord();
	    }

        public void trY(float trans, short num)
	    {
            if (num < 0)
                foreach (Atom atom in itsAtom)
                    atom.setTY(trans);
            else
                itsAtom[num].setTY(trans);
            nextCenterCoord();
	    }
        public void trZ(float trans, short num)
	    {
            if (num < 0)
                foreach (Atom atom in itsAtom)
                    atom.setTZ(trans);
            else
                itsAtom[num].setTZ(trans);
            nextCenterCoord();
	    }

        public static float toRadians(ref float angle)
        {
            return (float) ((Math.PI/180) * angle);
        }

	    public void rotX(float angle)
	    {
            float theta = toRadians(ref angle);
            float z, x;

	        foreach(Atom atom in itsAtom)
	        {
	            z = atom.TZ;
	            x = atom.TX;	            
	            atom.wrtTZ((float) (Zc + (z - Zc) * Math.Cos(theta) - (x - Xc) * Math.Sin(theta)));	            
	            atom.wrtTX((float) (Xc + (z - Zc) * Math.Sin(theta) + (x - Xc) * Math.Cos(theta)));
	        }
	    }

	    public void rotY(float angle)
	    {
            float theta = toRadians(ref angle);
            float y, z;
	        
            foreach(Atom atom in itsAtom)
	        {
	            y = atom.TY;
	            z = atom.TZ;
	            atom.wrtTY((float) (Yc + (y - Yc) * Math.Cos(theta) - (z - Zc) * Math.Sin(theta)));
	            atom.wrtTZ((float) (Zc + (y - Yc) * Math.Sin(theta) + (z - Zc) * Math.Cos(theta)));
	        }
	    }
	    public void rotZ(float angle)
	    {
		    float theta = toRadians(ref angle);
            float x, y;

	        foreach(Atom atom in itsAtom)
	        {
	            x = atom.TX;
	            y = atom.TY;
	            atom.wrtTX((float)(Xc + (x - Xc) * Math.Cos(theta) - (y - Yc) * Math.Sin(theta)));
	            atom.wrtTY((float)(Yc + (x - Xc) * Math.Sin(theta) + (y - Yc) * Math.Cos(theta)));
	        }
	    }
	    private void nextCenterCoord()
	    {
	        foreach(Atom atom in itsAtom)
	        {
	            Xc = Xc + atom.TX;
	            Yc = Yc + atom.TY;
	            Zc = Zc + atom.TZ;
	        }
	        Xc = Xc / M;
	        Yc = Yc / M;
	        Zc = Zc / M;
	    }

	    public void firstCenterCoord()
	    {
            foreach (Atom atom in itsAtom)
            {
                Xc = Xc + atom.X;
                Yc = Yc + atom.Y;
                Zc = Zc + atom.Z;
            }
            Xc = Xc / M;
            Yc = Yc / M;
            Zc = Zc / M;

            foreach (Atom atom in itsAtom)            
                atom.setDispCoord(Xc, Yc, Zc);                      
	    }
        
        private void backCenterCoord()
        {
            foreach (Atom atom in itsAtom)
            {
                atom.setX(Xc - atom.TX);
                atom.setY(Yc - atom.TY);
                atom.setZ(Zc - atom.TZ);
            }
        }

	    public void Docking(Atom a2, ushort segAmount, string filename)
	    {            
            Atom step = new Atom();

            step.wrtTX((a2.TX - itsAtom[Current].TX) / segAmount);
            step.wrtTY((a2.TY - itsAtom[Current].TY) / segAmount);
            step.wrtTZ((a2.TZ - itsAtom[Current].TZ) / segAmount);

            String[] str = new String[M*2+1];
            
            string path = @"result\" + DateTime.Now.Hour +"_"+ DateTime.Now.Minute +"_"+DateTime.Now.Second;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            ushort n;
           // StringBuilder sb = new StringBuilder();
            for (ushort i = 0; i < segAmount+1; i++)
            {   
                n = 0;                             
                for (ushort j = 0; j < M; j++)
                {                    
                    str[n] = "HETATM" + itsAtom[j].Num.ToString().PadLeft(5)                    
                        + itsAtom[j].Name.ToString().PadLeft(5)
                        + Strings.Format((itsAtom[j].TX + step.TX * i),"F3").PadLeft(22)
                        + Strings.Format((itsAtom[j].TY + step.TY * i),"F3").PadLeft(8)
                        + Strings.Format((itsAtom[j].TZ + step.TZ * i),"F3").PadLeft(8);                    
                    n++;
                }
                for (ushort j = 0; j < M; j++)
                {
                    str[n] = "CONECT" + itsAtom[j].Num.ToString().PadLeft(5)
                        + itsAtom[j].getNextAtom(0).ToString().PadLeft(5)
                        + itsAtom[j].getNextAtom(1).ToString().PadLeft(5)
                        + itsAtom[j].getNextAtom(2).ToString().PadLeft(5)
                        + itsAtom[j].getNextAtom(0).ToString().PadLeft(5);                    
                    n++;
                }
                str[n] = "END";

                File.WriteAllLines(path+@"\" + i + ".pdb", str);                  
            }                       
	    }


        public void paintSphereSel(ref int n)
        {            
            ushort i = 0;
            foreach (Atom atom in itsAtom)
            {                     
                Gl.glPushMatrix();                
                Gl.glTranslatef(atom.TX, atom.TY, atom.TZ);
                Gl.glLoadName(n);
                Glut.glutSolidSphere(Radius, 10, 10);
                Gl.glPopMatrix();
                i++;
                n++;                
            }            
        }
        public void paintSphereCur()
        {
            paintFrameWork(1.5F);
            for (ushort i = 0; i < itsAtom.Length; i++)
            {
                if (i == Current)
                    choiceColor(i, true);
                else
                    choiceColor(i, false);
                Gl.glPushMatrix();
                Gl.glTranslatef(itsAtom[i].TX, itsAtom[i].TY, itsAtom[i].TZ);
                Glut.glutSolidSphere(Radius, Slices, Slices);

                Gl.glPopMatrix();
            }
        }
        public static void outText(String text)
        {
            char[] ch = text.ToCharArray();
            Gl.glPushAttrib(Gl.GL_LIST_BIT);
            Gl.glListBase(1000);
            Gl.glCallLists(ch.Length, Gl.GL_UNSIGNED_BYTE, ch);
            Gl.glPopAttrib();
        }
        public void paintSphere()
        {
            paintFrameWork(1.5F);
            for (ushort i = 0; i < itsAtom.Length; i++)
            {
                choiceColor(i, false);
                Gl.glPushMatrix();
                Gl.glTranslatef(itsAtom[i].TX, itsAtom[i].TY, itsAtom[i].TZ);
                Glut.glutSolidSphere(Radius, Slices, Slices);
                          
                Gl.glColor3d(1, 1, 1);
                Gl.glTranslatef(-0.05F, -0.05F, -0.05F);
                Gl.glScalef(0.2F, 0.2F, 0.2F);
                outText("" + i);
 
                Gl.glPopMatrix();                
            }
        }

        public void paintFrameWork(float LWidth)
        {
            Gl.glLineWidth(LWidth);
            byte bak = 0;
            foreach (Atom atom in itsAtom)
                for (byte j = 1; j < 4; j++)
                {
                    bak = (byte)(j - 1);
                    if ((atom.getNextAtom(bak) == atom.Num) || (atom.getNextAtom(bak) == 0))
                        break;

                    float x = (itsAtom[atom.getNextAtom(bak) - 1].TX - atom.TX) / 2;
                    float y = (itsAtom[atom.getNextAtom(bak) - 1].TY - atom.TY) / 2;
                    float z = (itsAtom[atom.getNextAtom(bak) - 1].TZ - atom.TZ) / 2;

                    Gl.glPushMatrix();
                    Gl.glTranslatef(atom.TX, atom.TY, atom.TZ);
                    choiceColor((ushort)(atom.Num - 1),false);
                                       
                    setAngle(x, y, z);
                    Gl.glTranslatef(x, y, z);
                    choiceColor((ushort)(atom.getNextAtom(bak) - 1),false);
                    
                    setAngle(x, y, z);
                    Gl.glPopMatrix();
                }
        }
        private void setAngle(float x, float y, float z)
        {

            float p = (float)Math.Sqrt(x * x + y * y + z * z);
            float fi = (float)(Math.Atan(y / x) * 180 / Math.PI) ;
            float thet = (float)(Math.Acos(z / p) * 180 / Math.PI);

            Gl.glPushMatrix();
            if ((x < 0) && (y > 0))
                Gl.glRotatef(fi + 180, 0, 0, 1);
            else
                if ((x < 0) && (y < 0))
                    Gl.glRotatef(fi - 180, 0, 0, 1);
                else
                    Gl.glRotatef(fi, 0, 0, 1);

            Gl.glRotatef(thet, 0, 1, 0);
            Glut.glutSolidCylinder(Radius/3, p, Slices, Slices);            
            Gl.glPopMatrix();
        }

        private void choiceColor(ushort i, bool b)
        {
            float[] amb = null;
            float[] dif = null;
            float[] spec = null;
            float shine = 0;
            
            if (itsAtom[i].Name == null)
                itsAtom[i].setName("def");

            if (b == false)
                switch ("" + itsAtom[i].Name[0])
                {
                    case "H":
                        amb = new float[] { 0.1F, 0.19F, 0.7F };
                        dif = new float[] { 0.4F, 0.75F, 0.7F };
                        spec = new float[] { 0.3F, 0.3F, 0.3F };
                        shine = 0.1F; break;
                    case "C":
                        amb = new float[] { 0.2F, 0.2F, 0.2F };
                        dif = new float[] { 0.5F, 0.55F, 0.5F };
                        spec = new float[] { 0.5F, 0.5F, 0.5F };
                        shine = 0.4F; break;
                    case "O":
                        amb = new float[] { 0.1F, 0.1F, 0.1F };
                        dif = new float[] { 1, 0.3F, 0.3F };
                        spec = new float[] { 0.5F, 0.3F, 0.3F };
                        shine = 0.3F; break;
                    case "N":
                        amb = new float[] { 0.0F, 0.1F, 0.6F };
                        dif = new float[] { 0.0F, 0.5F, 0.5F };
                        spec = new float[] { 0.5F, 0.5F, 0.5F };
                        shine = 0.25F; break;
                    default:
                        amb = new float[] { 0.25F, 0.2F, 0.2F };
                        dif = new float[] { 1, 0.8F, 0.3F };
                        spec = new float[] { 0.3F, 0.3F, 0.3F };
                        shine = 0.9F; break;
                }
            else
            {
                amb = new float[] { 0.1F, 0.1F, 0.1F };
                dif = new float[] { 0.5F, 0.8F, 0.2F };
                spec = new float[] { 0.5F, 0.3F, 0.3F };
                shine = 0.3F;
            }

            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_AMBIENT, amb);
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_DIFFUSE, dif);
            Gl.glMaterialfv(Gl.GL_FRONT, Gl.GL_SPECULAR, spec);
            Gl.glMaterialf(Gl.GL_FRONT, Gl.GL_SHININESS, shine * 128.0F);
        }

        public Molecule(string fname)
        {
            string[] astr;          
            try
            {
                astr = File.ReadAllLines(fname);                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                return;
            }
            addPDB(astr);

        }        

        private void addPDB(string[] astr)
        {
            const string atom = "ATOM";
            const string hetatm = "HETATM";
            const string endfile = "END";
            const string conect = "CONECT";

            ushort amount = 0;
            string buf = "";
            ArrayList list = new ArrayList();
          
            foreach (string line in astr)
            {
                buf = Strings.Trim(Strings.Left(line, 6));
                if (buf == endfile)
                    break;
                if ((buf == atom) || (buf == hetatm))
                {
                    list.Add(line);                  
                    amount++;
                }
            }

            createMol(amount);
            ushort i = 0;
            ArrayList err_list = new ArrayList();            

            foreach (string line in list)
            {
                try
                {
                    buf = Strings.Trim(Strings.Mid(line, 7, 5));
                    itsAtom[i].setNum(Convert.ToUInt16(buf));

                    buf = Strings.Trim(Strings.Mid(line, 13, 4));
                    itsAtom[i].setName(buf);

                    buf = (Strings.Trim(Strings.Mid(line, 31, 8))).Replace(".", ",");
                    itsAtom[i].setTX(float.Parse(buf));

                    buf = (Strings.Trim(Strings.Mid(line, 39, 8))).Replace(".", ",");
                    itsAtom[i].setTY(float.Parse(buf));

                    buf = (Strings.Trim(Strings.Mid(line, 47, 8))).Replace(".", ",");
                    itsAtom[i].setTZ(float.Parse(buf));
                }
                catch (Exception e)
                {
                    err_list.Add(line);
                    err_list.Add("Generic Exception Handler: " + e.ToString());
                }
                finally { i++; }
            }

            buf = "";
            list = new ArrayList();
            foreach (string line in astr)
            {
                buf = Strings.Trim(Strings.Left(line, 6));
                if (buf == endfile)
                    break;
                else if (buf == conect)
                    list.Add(line);
            }
            amount = (ushort)list.Count;
            ushort[,] mat = new ushort[amount, 5];
            i = 0;
            foreach (string line in list)
            {
                try
                {
                    buf = Strings.Trim(Strings.Mid(line, 7, 5));
                    if (buf != "")
                        mat[i, 0] = (ushort)(Convert.ToUInt16(buf));

                    buf = Strings.Trim(Strings.Mid(line, 12, 5));
                    if (buf != "")
                        mat[i, 1] = (ushort)(Convert.ToUInt16(buf));

                    buf = (Strings.Trim(Strings.Mid(line, 17, 5)));
                    if (buf != "")
                        mat[i, 2] = (ushort)(Convert.ToUInt16(buf));

                    buf = (Strings.Trim(Strings.Mid(line, 22, 5)));
                    if (buf != "")
                        mat[i, 3] = (ushort)(Convert.ToUInt16(buf));

                    buf = (Strings.Trim(Strings.Mid(line, 27, 5)));
                    if (buf != "")
                        mat[i, 4] = (ushort)(Convert.ToUInt16(buf));
                }
                catch (Exception e)
                {
                    err_list.Add(line);
                    err_list.Add("Generic Exception Handler: " + e.ToString());
                }
                finally { i++; }
            }
            
            if (err_list.Count != 0)
            {
                using (StreamWriter sw = File.CreateText(@"error.log"))
                    sw.Write(err_list.ToArray());                                    
            }

            for (i = 0; i < mat.Length / 5; i++)
                for (ushort j = 1; j < 5; j++)
                    for (ushort n = 1; n < mat.Length / 5; n++)
                        if (mat[i, j] == mat[n, 0])
                        {
                            for (ushort a = 1; a < 5; a++)
                                if (mat[n, a] == mat[i, j - 1])
                                {
                                    mat[n, a] = 0;
                                    break;
                                }
                            break;
                        }

            foreach (Atom a in itsAtom)
                for (ushort j = 0; j < mat.Length / 5; j++)
                    if (a.Num == mat[j, 0])
                        for (i = 1; i < 5; i++)
                            a.setNextAtom((ushort)(i - 1), mat[j, i]);
            
            Array.Sort(itsAtom);

//            firstCenterCoord();
            //nextCenterCoord();            
        }

        
        public void setName(string name) { Name = name; }
        public void setRadius(float radius) { Radius = radius; }
        public void setSlices(int slices) { Slices = slices; }



        public ushort M { get; private set; }
        public string Name { get; private set; }

        public float Xc { get; private set; }
        public float Yc { get; private set; }
        public float Zc { get; private set; }

        public bool IsSelect;
        public int Current;

        public float Radius { get; private set; }
        public Atom[] itsAtom;
        public int Slices { get; private set; }
        
        //private Glu.GLUquadric quadObj;
    }
}
