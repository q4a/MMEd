using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

// A 3-vector with short components

namespace MMEd.Chunks
{
  public class Short3Coord
  {
    public short X, Y, Z;

    /// <summary>
    ///  Reads a Short3Coord from the stream, followed by two
    ///  zero bytes (i.e. so the structure is 64 bits)
    /// </summary>
    public static Short3Coord ReadShort3Coord64(BinaryReader bin)
    {
      Short3Coord acc = new Short3Coord();
      acc.X = bin.ReadInt16();
      acc.Y = bin.ReadInt16();
      acc.Z = bin.ReadInt16();
      if (bin.ReadInt16() != 0)
        throw new DeserialisationException("Expecting two zero bytes", bin.BaseStream.Position);
      return acc;
    }

    // see ReadShort3Coord64
    public void WriteShort3Coord64(BinaryWriter bout)
    {
      bout.Write(X);
      bout.Write(Y);
      bout.Write(Z);
      bout.Write((short)0);
    }

    public Short3Coord()
    {
    }

    public Short3Coord(short x, short y, short z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public static implicit operator Short3Coord(GLTK.Point xiPoint)
    {
      return new Short3Coord((short)xiPoint.x, (short)xiPoint.y, (short)xiPoint.z);
    }

    public static implicit operator Short3Coord(GLTK.Matrix xiRotation)
    {
      // Algorithm is as follows:
      //
      // There are only two degrees of freedom, so one of the coordinates in the
      // vector is redundant.  Therefore we fix one of the coordinates at 0 and
      // solve for the other 2.
      //
      //  * First identify the axis that moves the furthest under the rotation;
      //    this will be the fixed coordinate i.e. we'll rotate about the other
      //    two.
      //  * Next see what effect the rotation has on the axis that moves furthest;
      //    since rotations are defined uniquely by their action on a single point
      //    on the unit ball (apart from the poles) we just need to solve the
      //    simultaneous equations generated by applying rotations of theta and phi
      //    around the two non-fixed axis to this vector.  Note: it is sufficient
      //    to solve only two of the three equations as the third coordinate of the
      //    vector is determined by the other two.

      // see what effect the rotation has on the x and y axes
      GLTK.Vector lNewXAxis = xiRotation * GLTK.Vector.XAxis;
      GLTK.Vector lNewYAxis = xiRotation * GLTK.Vector.YAxis;
      GLTK.Vector lNewZAxis = xiRotation * GLTK.Vector.ZAxis;

      double lXdist = Math.Abs(lNewXAxis * GLTK.Vector.XAxis);
      double lYdist = Math.Abs(lNewYAxis * GLTK.Vector.YAxis);
      double lZdist = Math.Abs(lNewZAxis * GLTK.Vector.ZAxis);

      if (lXdist < lYdist && lXdist < lZdist)
      {
        // x axis has moved the furthest - use (0, y, z) format

        // rotate about z by theta first, then y by phi
        // need to solve cos(theta)*cos(phi) = newx.x; sin(theta) = newx.y
        double lTheta = Math.Asin(-lNewXAxis.y);

        // check which of the possible values for theta is correct
        if (lNewXAxis.x < 0) lTheta = Math.PI - lTheta;

        double lPhi = Math.Acos(lNewXAxis.x / Math.Cos(lTheta));

        // check which of the possible values for phi is correct
        if (lNewXAxis.z < 0) lPhi = -lPhi;

        return new Short3Coord(
          0,
          (short)((lPhi / Math.PI) * 2 * 1024),
          (short)((lTheta / Math.PI) * 2 * 1024));
      }
      else if (lYdist < lZdist)
      {
        // y axis has moved the furthest - use (x, 0, z) format

        // rotate about z by theta first, then x by phi
        // need to solve -sin(theta) = newy.x; cos(theta)cos(phi) = newy.y
        double lTheta = Math.Asin(lNewYAxis.x);

        // check which of the possible values for theta is correct
        if (lNewYAxis.y < 0) lTheta = Math.PI - lTheta;

        double lPhi = Math.Acos(lNewYAxis.y / Math.Cos(lTheta));

        // check which of the possible values for phi is correct
        if (lNewYAxis.z > 0) lPhi = -lPhi;

        return new Short3Coord(
          (short)((lPhi / Math.PI) * 2 * 1024),
          0,
          (short)((lTheta / Math.PI) * 2 * 1024));
      }
      else
      {
        // z axis has moved the furthest - use (x, y, 0) format

        // rotate about y by theta first, then x by phi
        // need to solve sin(theta) = newz.x; cos(phi)cos(theta) = newz.z
        double lTheta = Math.Asin(lNewZAxis.x);

        // check which of the possible values for theta is correct
        if (lNewZAxis.z < 0) lTheta = Math.PI - lTheta;

        double lPhi = Math.Acos(lNewZAxis.z / Math.Cos(lTheta));

        // check which of the possible values for phi is correct
        if (lNewZAxis.y < 0) lPhi = -lPhi;

        return new Short3Coord(
          (short)((lPhi / Math.PI) * 2 * 1024),
          (short)((lTheta / Math.PI) * 2 * 1024),
          0);
      }
    }

    public double Norm()
    {
      double dX = (double)X;
      double dY = (double)Y;
      double dZ = (double)Z;
      return Math.Sqrt(dX * dX + dY * dY + dZ * dZ);
    }

    public override string ToString()
    {
      return string.Format("({0},{1},{2})", X, Y, Z);
    }

    public static Short3Coord  operator +(Short3Coord x, Short3Coord y)
    {
      return new Short3Coord((short)(x.X + y.X), (short)(x.Y + y.Y), (short)(x.Z + y.Z));
    }

    public GLTK.Point ToPoint()
    {
      return new GLTK.Point(X, Y, Z);
    }

    public GLTK.Matrix ToRotationMatrix()
    {
      GLTK.Matrix lRotation = GLTK.Matrix.Rotation(-Z / 1024.0 * Math.PI / 2.0, GLTK.Vector.ZAxis);
      lRotation *= GLTK.Matrix.Rotation(-Y / 1024.0 * Math.PI / 2.0, GLTK.Vector.YAxis);
      lRotation *= GLTK.Matrix.Rotation(-X / 1024.0 * Math.PI / 2.0, GLTK.Vector.XAxis);

      return lRotation;
    }
  }
}
