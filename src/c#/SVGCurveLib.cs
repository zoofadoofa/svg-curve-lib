using System;

public class Vector2
{
    public double x;
    public double y;

    public Vector2(double x, double y)
    {
        this.x = x;
        this.y = y;
    }
}

// (zoofadoofa): not a full implementation of the lib, im working in a Unity3d project.
// so for this C# port to SVGCurveLib I converted floats to doubls and used System.Math instead of UnityEngine.Mathf
// I also mocked a Vector2 class above ^^^
public class SVGCurveLib
{
    private const double radians = Math.PI / 180;
    private const double PI2 = Math.PI * 2;
    public Vector2 PointOnEllipticalArc(
        Vector2 p0,
        double rx,
        double ry,
        double xAxisRotation,
        bool largeArcFlag,
        bool sweepFlag,
        Vector2 p1,
        double t
    )
    {
        // In accordance to: http://www.w3.org/TR/SVG/implnote.html#ArcOutOfRangeParameters
        rx = Math.Abs(rx);
        ry = Math.Abs(ry);


        // If the endpoints are identical, then this is equivalent to omitting the elliptical arc segment entirely.
        if (p0.x == p1.x && p0.y == p1.y)
        {
            return p0;
        }

        // If rx = 0 or ry = 0 then this arc is treated as a straight line segment joining the endpoints. 
        if (rx == 0 || ry == 0)
        {
            //(zoofadoofa): Not Implemented
            // return this.pointOnLine(p0, p1, t); 
        }

        // Following "Conversion from endpoint to center parameterization"
        // http://www.w3.org/TR/SVG/implnote.html#ArcConversionEndpointToCenter

        // Step #1: Compute transformedPoint
        double xAngle = radians * (xAxisRotation % 360D);
        double cosXAngle = Math.Cos(xAngle);
        double sinXAngle = Math.Sin(xAngle);

        double dx = (p0.x - p1.x) / 2;
        double dy = (p0.y - p1.y) / 2;
        Vector2 transformedPoint = new Vector2(
            cosXAngle * dx + sinXAngle * dy,
            -sinXAngle * dx + cosXAngle * dy
        );
        Vector2 transformedPointSquare = new Vector2(
            Math.Pow(transformedPoint.x, 2),
            Math.Pow(transformedPoint.y, 2)
        );

        // Ensure radii are large enough
        double radiiCheck = transformedPointSquare.x / Math.Pow(rx, 2) + transformedPointSquare.y / Math.Pow(ry, 2);
        if (radiiCheck > 1)
        {
            rx = Math.Sqrt(radiiCheck) * rx;
            ry = Math.Sqrt(radiiCheck) * ry;
        }

        // Step #2: Compute transformedCenter
        double rxSquare = Math.Pow(rx, 2);
        double rySqaure = Math.Pow(ry, 2);
        double cSquareNumerator = rxSquare * rySqaure - rxSquare * transformedPointSquare.y - rySqaure * transformedPointSquare.x;
        double cSquareRootDenom = rxSquare * transformedPointSquare.y + rySqaure * transformedPointSquare.x;
        double cRadicand = cSquareNumerator / cSquareRootDenom;
        // Make sure this never drops below zero because of precision
        cRadicand = cRadicand < 0 ? 0 : cRadicand;
        double cCoef = (largeArcFlag != sweepFlag ? 1 : -1) * Math.Sqrt(cRadicand);
        Vector2 transformedCenter = new Vector2(
            cCoef * ((rx * transformedPoint.y) / ry),
            cCoef * (-(ry * transformedPoint.x) / rx)
        );
        Vector2 center = new Vector2(
            cosXAngle * transformedCenter.x - sinXAngle * transformedCenter.y + ((p0.x + p1.x) / 2),
            sinXAngle * transformedCenter.x + cosXAngle * transformedCenter.y + ((p0.y + p1.y) / 2)
        );

        // Step #4: Compute start/sweep angles
        // Start angle of the elliptical arc prior to the stretch and rotate operations.
        // Difference between the start and end angles
        Vector2 startVector = new Vector2(
            (transformedPoint.x - transformedCenter.x) / rx,
            (transformedPoint.y - transformedCenter.y) / ry
        );
        double startAngle = angleBetween(new Vector2(1, 0), startVector);
        Vector2 endVector = new Vector2(
            (-transformedPoint.x - transformedCenter.x) / rx,
            (-transformedPoint.y - transformedCenter.y) / ry
        );
        double sweepAngle = angleBetween(startVector, endVector);

        if (!sweepFlag && sweepAngle > 0)
        {
            sweepAngle -= PI2;
        }
        else if (sweepFlag && sweepAngle < 0)
        {
            sweepAngle += PI2;
        }
        // We use % instead of `mod(..)` because we want it to be -360deg to 360deg(but actually in radians)
        sweepAngle %= PI2;

        // From http://www.w3.org/TR/SVG/implnote.html#ArcParameterizationAlternatives
        double angle = startAngle + (sweepAngle * t);
        double ellipseComponentX = rx * Math.Cos(angle);
        double ellipseComponentY = ry * Math.Sin(angle);

        // Attach some extra info to use

        double ellipticalArcStartAngle = startAngle;
        double ellipticalArcEndAngle = startAngle + sweepAngle;
        Vector2 ellipticalArcCenter = center;
        double resultantRx = rx;
        double resultantRy = ry;

        //(zoofadoofa): I only used the point on the arc so I dont use the five vars above
        return new Vector2(
            cosXAngle * ellipseComponentX - sinXAngle * ellipseComponentY + center.x,
            sinXAngle * ellipseComponentX + cosXAngle * ellipseComponentY + center.y
        );
    }

    private double angleBetween(
        Vector2 v0,
        Vector2 v1
    )
    {
        double p = v0.x * v1.x + v0.y * v1.y;
        double n = Math.Sqrt((Math.Pow(v0.x, 2) + Math.Pow(v0.y, 2)) * (Math.Pow(v1.x, 2) + Math.Pow(v1.y, 2)));
        double sign = v0.x * v1.y - v0.y * v1.x < 0 ? -1 : 1;
        double angle = sign * Math.Acos(p / n);

        //double angle = Math.atan2(v0.y, v0.x) - Math.atan2(v1.y,  v1.x);

        return angle;
    }
}