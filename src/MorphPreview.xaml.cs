using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MorphTool
{
    /// <summary>
    /// Interaction logic for MorphPreview.xaml
    /// </summary>
    public partial class MorphPreview : UserControl
    {
        AxisAngleRotation3D rot_x;
        AxisAngleRotation3D rot_y;
        ScaleTransform3D zoom = new ScaleTransform3D(1, 1, 1);
        Transform3DGroup modelTransform, cameraTransform;
        DirectionalLight DirLight1 = new DirectionalLight();
        DirectionalLight DirLight2 = new DirectionalLight();
        PerspectiveCamera Camera1 = new PerspectiveCamera();
        Model3DGroup modelGroup = new Model3DGroup();
        Viewport3D myViewport = new Viewport3D();
        GeometryModel3D myHeadMesh = null;
        GeometryModel3D myBodyMesh = null;
        GeometryModel3D myEarsMesh = null;
        GeometryModel3D myTailMesh = null;
        GeometryModel3D myCustomMesh = null;
        MaterialGroup myMaterial = new MaterialGroup();

        public MorphPreview()
        {
            InitializeComponent();
            rot_x = new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0);
            rot_y = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);

            cameraTransform = new Transform3DGroup();
            cameraTransform.Children.Add(zoom);
            modelTransform = new Transform3DGroup();
            modelTransform.Children.Add(new RotateTransform3D(rot_y));
            modelTransform.Children.Add(new RotateTransform3D(rot_x));

            DirLight1.Color = Colors.White;
            DirLight1.Direction = new Vector3D(.5, -.5, -1);

            Camera1.FarPlaneDistance = 20;
            Camera1.NearPlaneDistance = 0.05;
            Camera1.FieldOfView = 45;
            Camera1.LookDirection = new Vector3D(0, 0, -3);
            Camera1.UpDirection = new Vector3D(0, 1, 0);
            ModelVisual3D modelsVisual = new ModelVisual3D();
            modelsVisual.Content = modelGroup;

            myViewport.Camera = Camera1;
            myViewport.Children.Add(modelsVisual);
            myViewport.Height = 610;
            myViewport.Width = 475;
            myViewport.Camera.Transform = cameraTransform;
            this.canvas1.Children.Insert(0, myViewport);

            Canvas.SetTop(myViewport, 0);
            Canvas.SetLeft(myViewport, 0);
            this.Width = myViewport.Width;
            this.Height = myViewport.Height;
        }

        MeshGeometry3D SimMesh(GEOM simgeom, float yOffset)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            Point3DCollection verts = new Point3DCollection();
            Vector3DCollection normals = new Vector3DCollection();
            PointCollection uvs = new PointCollection();
            Int32Collection facepoints = new Int32Collection();
            int indexOffset = 0;

            GEOM g = simgeom;

            for (int i = 0; i < g.numberVertices; i++)
            {
                float[] pos = g.getPosition(i);
                verts.Add(new Point3D(pos[0], pos[1] - (yOffset / 2f), pos[2]));
                float[] norm = g.getNormal(i);
                normals.Add(new Vector3D(norm[0], norm[1], norm[2]));
                float[] uv = g.getUV(i, 0);
                uvs.Add(new Point(uv[0], uv[1]));
            }

            for (int i = 0; i < g.numberFaces; i++)
            {
                int[] face = g.getFaceIndices(i);
                facepoints.Add(face[0] + indexOffset);
                facepoints.Add(face[1] + indexOffset);
                facepoints.Add(face[2] + indexOffset);
            }

            indexOffset += g.numberVertices;

            mesh.Positions = verts;
            mesh.TriangleIndices = facepoints;
            mesh.Normals = normals;
            mesh.TextureCoordinates = uvs;
            return mesh;
        }

        internal ImageBrush GetImageBrush(System.Drawing.Image image)
        {
            BitmapImage bmpImg = new BitmapImage();
            MemoryStream ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            bmpImg.BeginInit();
            bmpImg.StreamSource = ms;
            bmpImg.EndInit();
            ImageBrush img = new ImageBrush();
            img.ImageSource = bmpImg;
            img.Stretch = Stretch.Fill;
            img.TileMode = TileMode.None;
            img.ViewportUnits = BrushMappingMode.Absolute;
            return img;
        }

        public void Start_Mesh(GEOM headGeom, GEOM bodyGeom, GEOM earsGeom, GEOM tailGeom, GEOM customGeom, System.Drawing.Image skin, bool reset)
        {
            Cursor = Cursors.Arrow;
            myMaterial.Children.Clear();
            myMaterial.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.LightGray)));
            if (skin != null) myMaterial.Children.Add(new DiffuseMaterial(GetImageBrush(skin)));

            modelGroup.Children.Clear();
            modelGroup.Children.Add(DirLight1);

            float[] headDim = headGeom != null ? headGeom.GetHeightandDepth() : new float[] { 0, 0 };
            float[] bodyDim = bodyGeom != null ? bodyGeom.GetHeightandDepth() : new float[] { 0, 0 };
            float[] customDim = customGeom != null ? customGeom.GetHeightandDepth() : new float[] { 0, 0 };
            float modelHeight = (new float[] { headDim[0], bodyDim[0], customDim[0] }).Max();
            float modelDepth = (new float[] { headDim[1], bodyDim[1], customDim[1] }).Max();
            if (reset)
            {
                Camera1.Position = new Point3D(0, 0, modelHeight + (modelDepth * 2));
                sliderXMove.Value = 0;
                sliderYMove.Value = 0;
                sliderZoom.Value = -(modelHeight + (modelDepth * 2));
            }

            if (headGeom != null)
            {
                MeshGeometry3D myHead = SimMesh(headGeom, modelHeight);
                myHeadMesh = new GeometryModel3D(myHead, myMaterial);
                myHeadMesh.Transform = modelTransform;
                modelGroup.Children.Add(myHeadMesh);
            }
            if (bodyGeom != null)
            {
                MeshGeometry3D myBody = SimMesh(bodyGeom, modelHeight);
                myBodyMesh = new GeometryModel3D(myBody, myMaterial);
                myBodyMesh.Transform = modelTransform;
                modelGroup.Children.Add(myBodyMesh);
            }
            if (earsGeom != null)
            {
                MeshGeometry3D myEars = SimMesh(earsGeom, modelHeight);
                myEarsMesh = new GeometryModel3D(myEars, myMaterial);
                myEarsMesh.Transform = modelTransform;
                modelGroup.Children.Add(myEarsMesh);
            }
            if (tailGeom != null)
            {
                MeshGeometry3D myTail = SimMesh(tailGeom, modelHeight);
                myTailMesh = new GeometryModel3D(myTail, myMaterial);
                myTailMesh.Transform = modelTransform;
                modelGroup.Children.Add(myTailMesh);
            }
            if (customGeom != null)
            {
                MeshGeometry3D myCustom = SimMesh(customGeom, modelHeight);
                myCustomMesh = new GeometryModel3D(myCustom, myMaterial);
                myCustomMesh.Transform = modelTransform;
                modelGroup.Children.Add(myCustomMesh);
            }
        }

        public void Stop_Mesh()
        {
            modelGroup.Children.Clear();
        }

        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Camera1.Position = new Point3D(Camera1.Position.X, Camera1.Position.Y, -sliderZoom.Value);
        }

        private void sliderYMove_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Camera1.Position = new Point3D(Camera1.Position.X, -sliderYMove.Value, Camera1.Position.Z);
        }

        private void sliderXMove_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Camera1.Position = new Point3D(-sliderXMove.Value, Camera1.Position.Y, Camera1.Position.Z);
        }

        private void sliderYRot_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            rot_y.Angle = sliderYRot.Value;
        }

        private void sliderXRot_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            rot_x.Angle = sliderXRot.Value;
        }
    }
}
