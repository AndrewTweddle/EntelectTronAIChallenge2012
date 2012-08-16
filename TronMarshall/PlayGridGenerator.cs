using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Windows;

namespace TronMarshall
{
    public class PlayGridGenerator
    {
       

        public const double TEXTURE_CELL_CX = 128;
        public const double TEXTURE_CELL_CY = 128;
        public const double TEXTURE_IMAGE_CX = 128*5;
        public const double TEXTURE_IMAGE_CY = 128;


        public MeshGeometry3D FlattenedSphereGeometry
        {
            get
            {
                const double RADIUS_X = 1;
                const double RADIUS_Y = 0.5;
                const double RADIUS_Z = 1;

                const double START_Y_ANG = 20; //stops the points getting too squashed at the extremities of the sphere
                const double START_X_ANG = 20; //leave at 0 or north and south poles won't render as 1 vertex

                const double MIN_BLOCKDIAGONAL_SIZE = 0.007;
                const double MAX_BLOCKDIAGONAL_SIZE = 0.018;

                MeshGeometry3D mesh = new MeshGeometry3D();

                for (double y = START_Y_ANG; y <= 180 - START_Y_ANG; y += (180.00 - (START_Y_ANG * 2)) / (TronSdk.Common.BOARD_BLOCKS_Y - 1))
                {
                    for (double x = START_X_ANG; x <= 180 - START_X_ANG; x += (180.00 - (START_X_ANG * 2)) / (TronSdk.Common.BOARD_BLOCKS_X - 1))
                    {
                        //convert degrees to radians
                        double xr = x * Math.PI / 180.00;
                        double yr = y * Math.PI / 180.00;

                        //calculate centre of current point
                        Point3D centrePoint = new Point3D(
                            y==START_Y_ANG||y==180-START_Y_ANG?0: RADIUS_X * Math.Cos(xr) * Math.Sin(yr),
                            RADIUS_Y * Math.Cos(yr),
                            RADIUS_Z * Math.Sin(xr) * Math.Sin(yr));

                        //calculate the diagonal size to the point edges
                        double blockSize = RADIUS_Z * Math.Sin(xr) * Math.Sin(yr) * MAX_BLOCKDIAGONAL_SIZE;
                        if (blockSize < MIN_BLOCKDIAGONAL_SIZE) blockSize = MIN_BLOCKDIAGONAL_SIZE;

                        defineGridBlock(mesh, centrePoint, blockSize);
                    }
                }

                return mesh;
            }
        }

        public MeshGeometry3D RectangularGeometry
        {
            get
            {
                const double SPACING_X=0.03;
                const double SPACING_Y = 0.03;
                const double BLOCKDIAGONAL_SIZE = 0.01;

                MeshGeometry3D mesh = new MeshGeometry3D();

                for (double y = 0; y < TronSdk.Common.BOARD_BLOCKS_Y ;y++)
                {
                    for (double x = 0; x < TronSdk.Common.BOARD_BLOCKS_X; x++)
                    {
                        //calculate centre of current point
                        Point3D centrePoint = new Point3D(
                            y == 0 || y == TronSdk.Common.BOARD_BLOCKS_Y-1 ? 0 : SPACING_X * (x-(TronSdk.Common.BOARD_BLOCKS_X/2.00)),
                            SPACING_Y * (y - (TronSdk.Common.BOARD_BLOCKS_Y / 2.00)),
                            0);


                        defineGridBlock(mesh, centrePoint, BLOCKDIAGONAL_SIZE);
                    }
                }

                return mesh;
            }
        }
        private void defineGridBlock(MeshGeometry3D mesh, Point3D centrePoint, double blockSize)
        {

            int i = mesh.Positions.Count;

            //create a set of vertexes about this point (square)
            mesh.Positions.Add(new Point3D(centrePoint.X - blockSize, centrePoint.Y - blockSize, centrePoint.Z));
            mesh.Positions.Add(new Point3D(centrePoint.X + blockSize, centrePoint.Y - blockSize, centrePoint.Z));
            mesh.Positions.Add(new Point3D(centrePoint.X + blockSize, centrePoint.Y + blockSize, centrePoint.Z));
            mesh.Positions.Add(new Point3D(centrePoint.X - blockSize, centrePoint.Y + blockSize, centrePoint.Z));

            //map the texture coordinates
            mesh.TextureCoordinates.Add(new Point(0, 0));
            mesh.TextureCoordinates.Add(new Point((PlayGridGenerator.TEXTURE_CELL_CX - 1) / PlayGridGenerator.TEXTURE_IMAGE_CX, 0));
            mesh.TextureCoordinates.Add(new Point((PlayGridGenerator.TEXTURE_CELL_CX - 1) / PlayGridGenerator.TEXTURE_IMAGE_CX, 1));
            mesh.TextureCoordinates.Add(new Point(0, 1));

            //define the triangles
            mesh.TriangleIndices.Add(i);
            mesh.TriangleIndices.Add(i + 1);
            mesh.TriangleIndices.Add(i + 2);

            mesh.TriangleIndices.Add(i + 2);
            mesh.TriangleIndices.Add(i + 3);
            mesh.TriangleIndices.Add(i);
        }

    }
}

