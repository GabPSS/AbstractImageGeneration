using System.Numerics;

namespace AbstractImageGeneration
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Simulation sim = new(0, Size.Empty, 0);
        private Size bounds = new(1, 1);
        private int padding = 300;
        private Color BaseColor = Color.White;
        private Bitmap bmp;
        public void Draw()
        {
            if (sim.Particles.Length != 0)
            {
                //Bitmap img = new(bounds.Width, bounds.Height);
                if (bmp == null)
                {
                    bmp = new Bitmap(bounds.Width, bounds.Height);
                }
                Graphics g = Graphics.FromImage(/*img*/bmp);
                List<MathStructures.Triangle> list = Triangulator.TriangulateByFlippingEdges(sim.Vector3Particles);
                if (radioButton1.Checked)
                {
                    g.Clear(Color.White);
                    for (int i = 0; i < list.Count; i++)
                    {
                        MathStructures.Triangle t = list[i];
                        g.FillPolygon(new SolidBrush(sim.GetColorForPoint(new Point(Convert.ToInt32(t.v1.position.X), Convert.ToInt32(t.v1.position.Z)), BaseColor)), new Point[]
                        {
                            new Point(Convert.ToInt32(t.v1.position.X - (padding / 2)), Convert.ToInt32(t.v1.position.Z - (padding / 2))),
                            new Point(Convert.ToInt32(t.v2.position.X - (padding / 2)), Convert.ToInt32(t.v2.position.Z - (padding / 2))),
                            new Point(Convert.ToInt32(t.v3.position.X - (padding / 2)), Convert.ToInt32(t.v3.position.Z - (padding / 2)))
                        });
                    }
                }
                else if (radioButton3.Checked)
                {
                    g.Clear(Color.White);
                    for (int i = 0; i < list.Count; i++)
                    {
                        MathStructures.Triangle t = list[i];
                        g.DrawPolygon(Pens.Black, new Point[]
                        {
                            new Point(Convert.ToInt32(t.v1.position.X - (padding / 2)), Convert.ToInt32(t.v1.position.Z - (padding / 2))),
                            new Point(Convert.ToInt32(t.v2.position.X - (padding / 2)), Convert.ToInt32(t.v2.position.Z - (padding / 2))),
                            new Point(Convert.ToInt32(t.v3.position.X - (padding / 2)), Convert.ToInt32(t.v3.position.Z - (padding / 2)))
                        });
                    }
                    for (int i = 0; i < sim.Particles.Length; i++)
                    {
                        g.FillEllipse(new SolidBrush(BaseColor), sim.Particles[i].Coordinate.X - (padding / 2) - 5, sim.Particles[i].Coordinate.Y - (padding / 2) - 5, 10, 10);
                    }
                }
                else if (radioButton2.Checked)
                {
                    for (int i = 0; i < sim.Particles.Length; i++)
                    {
                        g.FillEllipse(new SolidBrush(sim.Particles[i].ForeColor), sim.Particles[i].Coordinate.X - (padding / 2) - 50, sim.Particles[i].Coordinate.Y - (padding / 2) - 50, 100, 100);
                    }
                }
                sim.Iterate();
                g.FillRectangle(new SolidBrush(Color.FromArgb(255 - Convert.ToInt32(Convert.ToInt32(numericUpDown6.Value)*2.55), 0, 0, 0)), new Rectangle(Point.Empty, bounds));
                pictureBox1.Image = bmp;
            }
        }


        private void panel1_SizeChanged(object sender, EventArgs e)
        {

            //bounds = new Rectangle(Point.Empty, new Size(panel1.Size.Width + padding, panel1.Size.Height + padding));
            //Graphics g = panel1.CreateGraphics();
            //buffer = BufferedGraphicsManager.Current.Allocate(g, bounds);
            //if (sim != null)
            //{
            //    sim.Bounds = bounds.Size;
            //}
        }

        private void Generate()
        {
            padding = Convert.ToInt32(numericUpDown3.Value);
            Size paddedbounds = new(Convert.ToInt32(numericUpDown1.Value) + padding, Convert.ToInt32(numericUpDown2.Value) + padding);
            bounds = new Size(Convert.ToInt32(numericUpDown1.Value), Convert.ToInt32(numericUpDown2.Value));
            sim = new(Convert.ToInt32(numericUpDown4.Value), paddedbounds, Convert.ToInt32(numericUpDown5.Value));
            bmp = new Bitmap(bounds.Width, bounds.Height);
            Draw();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Generate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Draw();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            timer1.Enabled = checkBox1.Checked;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Draw();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Generate();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            Generate();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            Generate();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            Generate();
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            Generate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(pictureBox1.Image);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                BaseColor = colorDialog1.Color;
                Draw();
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Generate();
        }
    }

    public class Simulation
    {
        public Particle[] Particles;
        public List<Vector3> Vector3Particles
        {
            get
            {
                List<Vector3> particles = new();
                for (int i = 0; i < Particles.Length; i++)
                {
                    particles.Add(new Vector3(Particles[i].Coordinate.X, 0, Particles[i].Coordinate.Y));
                }
                return particles;
            }
        }
        public Size Bounds;
        public Simulation(int size, Size bounds, int speed)
        {
            Particles = Array.Empty<Particle>();
            Bounds = bounds;
            Random r = new();
            for (int i = 0; i < size; i++)
            {
                Particles = Particles.Append(new(r.Next(bounds.Width), r.Next(bounds.Height)) { Speed = new(r.Next(-speed, speed), r.Next(-speed, speed)) }).ToArray();
            }
        }

        public void Iterate()
        {
            for (int i = 0; i < Particles.Length; i++)
            {
                Particles[i].ComputeNext(Bounds);
            }
        }

        public Color GetColorForPoint(Point p, Color basecol)
        {
            try
            {
                int w = p.X;
                int h = p.Y;

                double green = Bounds.Height - h;

                //Calculate Big S

                double tg = Convert.ToDouble(Bounds.Width) / Convert.ToDouble(Bounds.Height);
                double a_angle_tan = Math.Atan(tg); ///HOW??????
                double total_max = Bounds.Height / Math.Cos(a_angle_tan);

                double b_angle_rad = (Math.PI / 2) - Math.Pow(Math.Atan(tg), -1);

                double part1 = Math.Tan(b_angle_rad) * w;

                double part2 = green / Math.Cos(a_angle_tan);

                double total_min = part1 + part2;

                double distance = part1 * total_max / total_min;

                distance /= total_max;
                distance *= 210;
                distance += 45;
                distance /= 255;
                if (distance > 1)
                {
                    distance = 1;
                }
                Color col = Color.FromArgb(Convert.ToInt32(Convert.ToDouble(basecol.R) * distance), Convert.ToInt32(Convert.ToDouble(basecol.G) * distance), Convert.ToInt32(Convert.ToDouble(basecol.B) * distance));

                return col;
            }
            catch { return Color.Black; }
        }
    }

    public struct Speed
    {
        public int X;
        public int Y;

        public Speed(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class Particle
    {
        public Point Coordinate { get; set; }
        public Speed Speed { get; set; } = new Speed(0, 0);
        public int Size = 10;
        public Color ForeColor { get; set; }
        public Particle(int x, int y)
        {
            Coordinate = new Point(x, y);
            Random r = new();
            ForeColor = Color.FromArgb(r.Next(256), r.Next(256), r.Next(256));
        }

        public void ComputeNext(Size bounds)
        {

            Speed newSpeed = Speed;
            if (Coordinate.X > bounds.Width || Coordinate.X == 0)
            {
                newSpeed.X = -newSpeed.X;
            }
            if (Coordinate.Y > bounds.Height || Coordinate.Y == 0)
            {
                newSpeed.Y = -newSpeed.Y;
            }
            Speed = newSpeed;
            Point newPoint = new(Coordinate.X + Speed.X, Coordinate.Y + Speed.Y);

            if (Coordinate.X > bounds.Width)
            {
                newPoint.X = bounds.Width;
            }
            if (Coordinate.Y > bounds.Height)
            {
                newPoint.Y = bounds.Height;
            }
            if (Coordinate.X < 0)
            {
                newPoint.X = 0;
            }
            if (Coordinate.Y < 0)
            {
                newPoint.Y = 0;
            }
            Coordinate = newPoint;
        }
    }
}