using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
//using System.Text;
using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform.Windows;

namespace MolGL
{
    public partial class Form1 : Form
    {
        private List<Molecule> Mol;              
        private float lastX, lastY;
        private float trans;
        private int count;
        private int currentObject;
        private const uint PICK_OBJECTS = 8;
        private int[] selBuf;
        private int[] viewport;
        
        private bool IsShift;
        private bool IsCtrl;        
        private bool IsAlt;
        private bool IsSpace;
        private ushort numMol;
        private ushort i;

        public Form1()
        {            
            currentObject = -1;
            viewport = new int[4];
            selBuf = new int[4];
            trans = -8;
            Mol = new List<Molecule>();
            InitializeComponent();
            AnT.InitializeContexts();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Size = new Size((SystemInformation.PrimaryMonitorSize.Width / 4) * 3,
                (SystemInformation.PrimaryMonitorSize.Height / 4) * 3);
            CenterToParent();

            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            //Wgl.wglUseFontOutlinesA(AnT.Handle, 0, 255, 1000, 50, 0.15F, Wgl.WGL_FONT_POLYGONS, null);
            //Wgl.wglUseFontOutlinesW(AnT.Handle,0,255,1000,                      
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_LIGHT0);

            MolAdd("Benzene.pdb");
            MolAdd("Ethane.pdb");
        }

        private void MolAdd(string fname)
        {
            Molecule m = new Molecule(fname);
            if (m.M != 0)
                Mol.Add(m);
        }
        private void MolFirst(string fname)
        {
            Molecule m = new Molecule(fname);
            if (m.M != 0)
            {
                Mol.Clear();
                Mol.Add(m);
            }

        }

        private void AnT_Paint(object sender, PaintEventArgs e)
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            //paintOS();
            Render(Gl.GL_RENDER);

            Gl.glFlush();
            AnT.Invalidate();
        }

        private void pushObjects()
        {            
            Gl.glInitNames();
            Gl.glPushName(0);
            count = 1;
            Render(Gl.GL_SELECT);
        }

        private void AnT_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
                IsSpace = true;
            if (e.Shift)
                IsShift = true;
            if (e.Alt)
                IsAlt = true;
            if (e.Control && e.KeyCode == Keys.O)
                openFile(true);
            else if (e.Control && e.KeyCode == Keys.A)
                openFile(false);
            if (e.Control)
                IsCtrl = true;
            else if (e.KeyCode == Keys.R)
                reshape();
            else if (e.KeyCode == Keys.Add)
            {
                trans += 0.5F;
                reshape();
            }
            else if (e.KeyCode == Keys.Subtract)
            {
                trans -= 0.5F;
                reshape();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                //DialogResult result = MessageBox.Show("Вы де", "Внимание", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);

//                if (result == DialogResult.Yes)
                    Close();
            }
        }

        private void AnT_MouseDown(object sender, MouseEventArgs e)
        {
            pushObjects();
            currentObject = getSelBuf(e.X, e.Y) - 1;
            
            int res = 0;
            int oldres = 0;

            for(i=0; i < Mol.Count; i++)
                Mol[i].IsSelect = false;

            for (i = 0; i < Mol.Count; i++)
            {
                res += Mol[i].M;
                if (currentObject > res - 1)
                {
                    oldres = res;
                    continue;
                }
                else
                {                            
                    Mol[i].IsSelect = true;
                    Mol[i].Current = currentObject-oldres;
                    if (!IsShift)
                        numMol = i;
                    else
                    {
                        if (Mol[i].Current >= 0)
                        {
                            if (!textBox1.Visible)
                            {                                
                                //textBox1.Location = new Point(AnT.Width / 2-textBox1.Width/2,AnT.Height / 2-textBox1.Height/2);                                
                                //Point point = new Point(0, toolStrip.Height + menuStrip.Height);
                                //label1.Location = point;                                
                                //label1.Visible = true;                               
                                //label1.Text = "Введите кол. шагов:";
                                //point.X = point.X + label1.Width;
                                textBox1.Location = new Point(label1.Width, toolStrip.Height + menuStrip.Height);
                                textBox1.Clear();
                                label1.Visible = true;
                                textBox1.Visible = true;
                            }
                            textBox1.Focus();
                        }
                    }
                    break;    
                }
            }
            lastX = e.X;
            lastY = e.Y;               
        }
        private void AnT_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsSpace)
            {
                if (e.Button == MouseButtons.Left)
                {
                    foreach (Molecule mol in Mol)
                        if (mol.IsSelect)
                        {
                            mol.trY(-(e.Y - lastY) / 70, (short)mol.Current);
                            mol.trX((e.X - lastX) / 70, (short)mol.Current);
                            break;
                        }
                }
                else if (e.Button == MouseButtons.Right)              
                    foreach (Molecule mol in Mol)
                        if (mol.IsSelect)
                        {
                            mol.trZ((e.X + e.Y - lastX - lastY) / 20, (short)mol.Current);
                            break;
                        }                
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (IsAlt && IsCtrl)
                {
                    foreach (Molecule mol in Mol)
                        if (mol.IsSelect)
                        {
                            mol.trZ((e.X + e.Y - lastX - lastY) / 20, -1);
                            break;
                        }
                }
                else if (IsAlt)
                    Gl.glTranslatef(0, 0, (e.X + e.Y - lastX - lastY) / 20);
                else if (!IsCtrl)
                    Gl.glRotatef(e.X + e.Y - lastX - lastY, 0, 0, 1);
                else
                    foreach (Molecule mol2 in Mol)
                        if (mol2.IsSelect)
                        {
                            mol2.rotZ(e.X + e.Y - lastX - lastY);
                            break;
                        }
            }
            else if (e.Button == MouseButtons.Left)
            {
                if (IsAlt && IsCtrl)
                {
                    foreach (Molecule mol in Mol)
                        if (mol.IsSelect)
                        {
                            mol.trY(-(e.Y - lastY) / 70, -1);
                            mol.trX((e.X - lastX) / 70, -1);
                            break;
                        }
                }
                else if (IsAlt)
                {
                    Gl.glTranslatef(0, -(e.Y - lastY) / 70, 0);
                    Gl.glTranslatef((e.X - lastX) / 70, 0, 0);

                }
                else if (!IsCtrl)
                {
                    Gl.glRotatef(e.X - lastX, 0, 1, 0);
                    Gl.glRotatef(e.Y - lastY, 1, 0, 0);
                }
                else
                    foreach (Molecule mol in Mol)
                        if (mol.IsSelect)
                        {
                            mol.rotX(e.X - lastX);
                            mol.rotY(e.Y - lastY);
                            break;
                        }
            }
            foreach (Molecule mol in Mol)
                if (mol.IsSelect)
                {
                    if(mol.Current >= 0)
                    toolStripStatusLabel1.Text = "Atom №"+mol.itsAtom[mol.Current].Num.ToString()+": "
                        + Strings.Format(mol.itsAtom[mol.Current].TX,"F3").Replace(",",".") + ", "
                        + Strings.Format(mol.itsAtom[mol.Current].TY, "F3").Replace(",", ".") + ", "
                        + Strings.Format(mol.itsAtom[mol.Current].TZ, "F3").Replace(",", ".");
                    break;
                }
            lastX = e.X;
            lastY = e.Y;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ushort step;
                try
                {
                    step = Convert.ToUInt16(textBox1.Text);
                }
                catch 
                {
                    toolStripStatusLabel2.Text = "Не правильно введен шаг. Повторите операцию:";
                    textBox1.Focus();
                    return;
                }
                string filename = "result.txt";
                /*
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"  ;
                saveFileDialog1.FilterIndex = 2 ;
                saveFileDialog1.RestoreDirectory = true ;
                
                Stream myStream;
                if(saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        filename = saveFileDialog1.FileName;                        
                        myStream.Close();
                  */      
                        Mol[numMol].Docking(Mol[i].itsAtom[Mol[i].Current], step, filename);

                        numMol = i;
                        label1.Visible = false;
                        textBox1.Visible = false;
                        IsShift = false;
                        toolStripStatusLabel2.Text = "Docking OK.";
/*                    }
                    else
                    {
                        toolStripStatusLabel2.Text = "Не верно задано имя файла";
                        myStream.Close();
                        return;
                    }

                }
 */
            }
 
            else if (e.KeyCode == Keys.Escape)
            {
                textBox1.Visible = false;
                toolStripStatusLabel2.Text = "Docking Cancel";
            }
        }

        private void Render(int mode)
        {
            if (mode == Gl.GL_SELECT)
            {                
                Gl.glDisable(Gl.GL_DEPTH_TEST);
                Gl.glDisable(Gl.GL_LIGHTING);
                Gl.glDisable(Gl.GL_LIGHT0);            
                foreach (Molecule m in Mol)
                    if (m != null)
                        m.paintSphereSel(ref count);
            }
            else if (mode == Gl.GL_RENDER)
            {
                Gl.glEnable(Gl.GL_DEPTH_TEST);
                Gl.glEnable(Gl.GL_LIGHTING);
                Gl.glEnable(Gl.GL_LIGHT0);

                for (ushort i = 0; i < Mol.Count; i++)
                {
                    if (Mol[i].IsSelect)
                        Mol[i].paintSphereCur();
                    else
                        Mol[i].paintSphere();
                }
           }
        }
        private int getSelBuf(int x, int y)
        {
            int[] selBuf = new int[8];
            int[] viewport = new int[4];
            int Found = 0;

            Gl.glSelectBuffer(selBuf.Length, selBuf);
            Gl.glGetIntegerv(Gl.GL_VIEWPORT, viewport);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPushMatrix();
            Gl.glRenderMode(Gl.GL_SELECT);
            Gl.glLoadIdentity();
            
            Glu.gluPickMatrix(x, viewport[3] - y, 1, 1, viewport);
            Glu.gluPerspective(45, (float)AnT.Width / (float)AnT.Height, 0.1, 200);            
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            
            Gl.glPushMatrix();
                Gl.glLoadIdentity();
                Gl.glTranslatef(0, 0, trans);
            Gl.glPopMatrix();

                pushObjects();
                Found = Gl.glRenderMode(Gl.GL_RENDER);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            int selObject = 0;
            if (Found > 0)
            {
                int Depth = selBuf[1];
                selObject = (int) selBuf[3];
                for (int i = 1; i < Found; i++)                
                if (selBuf[(i * 4) + 1] < Depth)
                {
                    Depth = selBuf[(i * 4) + 1];
                    selObject = (int) selBuf[(i * 4) + 3];
                }                                
            }
            return selObject;
        }
        private void reshape()
        {
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glTranslatef(0, 0, trans);
        }

        private void AnT_Resize(object sender, EventArgs e)
        {
            Gl.glClearColor(0, 0, 0, 1);
            Gl.glViewport(0, 0, AnT.Width, AnT.Height);

            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPerspective(45, (float)AnT.Width / (float)AnT.Height, 0.1, 200);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glTranslatef(0, 0, trans);
        }

        private void openFile(bool b)
        {
            IsCtrl = false;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "././";
            openFileDialog1.Filter = "Protein Data Bank(*.pdb) | *.pdb";
            openFileDialog1.FilterIndex = 2;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (b)
                    Mol.Clear();
                MolAdd(openFileDialog1.FileName);
                statusStrip.Text = "OK!";
            }
            else
            {
                statusStrip.Text = "Операция отменена";
                return;
            }
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFile(false);
        }


        public static void paintOS()
        {
            float os = 1.5F;

            Gl.glPushMatrix();
            Gl.glTranslatef(os, 0, 0);
            Gl.glRotatef(-90, 0, 1, 0);
            Glut.glutSolidCylinder(0.02, os, 20, 20);
            Gl.glRotatef(180, 0, 1, 0);
            //Glut.glutSolidCylinder(0.04, 0.15, 20, 20);
            Glut.glutSolidCone(0.04, 0.15, 20, 20);
            Gl.glRotatef(-90, 0, 1, 0);
            Gl.glTranslatef(0, 0.04F, 0);
            Gl.glScalef(0.3F, 0.3F, 0.3F);
            //Molecule.outText("X");
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Gl.glTranslatef(0, os, 0);
            Gl.glRotatef(90, 1, 0, 0);
            Glut.glutSolidCylinder(0.02, os, 20, 20);
            Gl.glRotatef(180, 0, 1.0F, 0);
            Glut.glutSolidCone(0.04, 0.15, 20, 20);
            Gl.glRotatef(90, 1.0F, 0, 0);
            Gl.glTranslatef(0, 0.04F, 0);
            Gl.glScalef(0.3F, 0.3F, 0.3F);
            //Molecule.outText("Y");
            Gl.glPopMatrix();

            Gl.glPushMatrix();
            Glut.glutSolidCylinder(0.02, os, 20, 20);
            Gl.glTranslatef(0, 0, os);
            Glut.glutSolidCone(0.04, 0.15, 20, 20);
            Gl.glTranslatef(-0.06F, 0.04F, 0);
            Gl.glScalef(0.3F, 0.3F, 0.3F);
            //Molecule.outText("Z");
            Gl.glPopMatrix();
        }

        private void AnT_KeyUp(object sender, KeyEventArgs e)
        {
            if (IsSpace)
                IsSpace = false;
            if (IsShift)
                IsShift = false;
            if (IsCtrl)
                IsCtrl = false;
            if (IsAlt)
                IsAlt = false;
        }

        private void AnT_DragDrop(object sender, DragEventArgs e)
        {           
            try
            {
                string[] fname = (string[])e.Data.GetData(DataFormats.FileDrop);
                
                if(e.KeyState == 0)
                    Mol.Clear();

                foreach (string s in fname)
                    if (s != null)
                    {
                        MolAdd(s);
                    }                     
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in DragDrop function: " + ex.Message);
            }
   
        }

        private void AnT_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;

        }

        private void newToolStripButton_Click(object sender, EventArgs e)
        {
            openFile(false);
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            openFile(true);
        }


        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFile(false);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFile(true);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AnT_Load(object sender, EventArgs e)
        {

        }

        private void textBoxCommand_TextChanged(object sender, EventArgs e)
        {
            string sCommand = textBoxCommand.Text;
            if (sCommand[0] != '_')
            {
                toolStripStatusLabel1.Text = "No command";
                return;
            }
            char ch;
            System.Text.StringBuilder buf = new System.Text.StringBuilder();
            for (int i = 0; i < sCommand.Length; i++)
            {
                if (sCommand[i] != ' ')     
                {
                    buf.Append(sCommand[i]);                                    
                    sCommand.Remove(0,1);
                }
                else
                {
                    sCommand.Remove(0,1);
                    break;
                }
            }
            string sCom = buf.ToString();
            switch (sCom)
            {
                case "_dock":
                case "_rotate":                  
                case "_translate":
                    break;
            }                            
        }
    }
}
