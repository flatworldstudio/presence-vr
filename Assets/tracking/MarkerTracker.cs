
#define TRACKERDEBUG

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
//using System.Diagnostics;
//using NUnit.Framework.Internal.Commands;


namespace markerTracking2
{

    public class Tracker
    {


        public Texture2D debugTexture;
        //, texture;

        public int width, height;
        public Color[] pixels;

#if TRACKERDEBUG
        Color[] debugPixels;

#endif

        const int aTresh = 25;
        const int step = 4;

        const int outLimit = 4;

        const float MaxErrorSquared = 5000;

        List<Detection> detections;

        public List<Detection> markers;


        public Tracker()
        {
            markers = new List<Detection>();
        }



        public void TrackMarkers(WebCamTexture _texture)
        {
            
            // Detect marker candidates.

            DetectMarker(_texture.GetPixels(), _texture.width, _texture.height);

            List<Detection> newMarkers = new List<Detection>();

            for (int m = markers.Count-1; m >=0; m--)
            {
                Detection marker = markers[m];

                marker.tracking=false;

            }


            for (int d = 0; d < detections.Count; d++)
            {

                Detection detection = detections[d];

                bool FoundMarker = false;

                for (int m = 0; m < markers.Count; m++)
                {

                    Detection marker = markers[m];

                    if (detection.IsInSearchArea(marker))
                    {

                        marker.ApplyFrom(detection);
                        //Debug.Log("tracking");

                        FoundMarker = true;

                       
                    }

                }

                if (!FoundMarker){
                    newMarkers.Add(detection);
                }



            }

           
           
            for (int m = markers.Count-1; m >=0; m--)
            {
                Detection marker = markers[m];

                if (!marker.tracking){
                    
                    markers.RemoveAt(m);

                } else {

                    Vector2 bbox00 = new Vector2(marker.xArea0, marker.yArea0);
                    Vector2 bbox01 = new Vector2(marker.xArea0, marker.yArea1);
                    Vector2 bbox10 = new Vector2(marker.xArea1, marker.yArea0);
                    Vector2 bbox11 = new Vector2(marker.xArea1, marker.yArea1);

                    DrawLine(bbox00, bbox01, debugPixels, Color.cyan);
                    DrawLine(bbox01, bbox11, debugPixels, Color.cyan);
                    DrawLine(bbox11, bbox10, debugPixels, Color.cyan);
                    DrawLine(bbox10, bbox00, debugPixels, Color.cyan);
                }


            }

            markers.AddRange(newMarkers);


            debugTexture.SetPixels(debugPixels);
            debugTexture.Apply();

        }

        public void DetectMarker(WebCamTexture _texture)
        {

            DetectMarker(_texture.GetPixels(), _texture.width, _texture.height);
        }

        public void Set(Texture2D _texture)
        {

            width = _texture.width;
            height = _texture.height;
            pixels = _texture.GetPixels();


            //Detect(_texture.GetPixels(), _texture.width, _texture.height);


        }




        private void DetectMarker(Color[] _pixelData, int _width, int _height)

        //public IEnumerator Detect()

        {
            width = _width;
            height = _height;
            pixels = _pixelData;


            //Debug.Log("running");

#if TRACKERDEBUG

            if (debugTexture == null || debugTexture.width != width || debugTexture.height != height)
            {
                debugTexture = new Texture2D(width, height);
            }

            debugPixels = debugTexture.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {

                debugPixels[i] = 0.5f * pixels[i];
            }

#endif

            // Create a list of detections. Both markers and rejects.

            detections = new List<Detection>();

            for (int y = 0; y < height; y += step)
            {

                float oldA = LABColor.FromColor(pixels[y * width + 0]).a;

                for (int x = 0; x < width; x += step)
                {

                    float newA = LABColor.FromColor(pixels[y * width + x]).a;

                    // Compare A channels and check if point is in previous detections. If so, move x to the end of that detection for speed.

                    if (newA - oldA > aTresh && !IsInPreviousDets(ref x, ref y))
                    {

                        Detection cand = new Detection();

                        //Detection cand = OrthScan(x, y, newA, new Detection());

                        int limit = 0; // Safeguard against looping.

                        // Scan must stay in canvas and must be converging. If convergence is small enough we stop scanning.
                        int scanx = x;
                        int scany = y;

                        do
                        {

                            OrthScan(scanx, scany, newA, ref cand);
                            scanx = Mathf.RoundToInt(cand.innerCenter.x);
                            scany = Mathf.RoundToInt(cand.innerCenter.y);

                            limit++;

                        } while (cand.isValid && cand.convergingInner && cand.convergingOuter && cand.convergenceInner > 1 && limit < 10);




                        if (cand.convergenceInner >= 1)
                        {


                            //Debug.Log("scanning failed, reject candidate");


                        }
                        else
                        {

                            //Debug.LogWarning("scanning success");
                            //Debug.Log(cand.innerCenter.ToString());

                            debugPixels[Mathf.RoundToInt(cand.innerCenter.y) * width + Mathf.RoundToInt(cand.innerCenter.x)] = Color.cyan;

                            int borderx,bordery;
                            bool InnerFailed=false;

                            // Rescan initial points at 1 pixel accuracy.

                            //scanx = Mathf.RoundToInt(cand.innerCenter.x);
                            //scany = Mathf.RoundToInt(cand.innerCenter.y);

                            LineScan(scanx, scany, 0, -1, (value) => { return newA - value > aTresh; }, (xvalue, yvalue) => { return yvalue < 0; }, out borderx, out cand.iy0, ref InnerFailed);
                            LineScan(scanx, scany, 0, 1, (value) => { return newA - value > aTresh; }, (xvalue, yvalue) => { return yvalue >= height; }, out borderx, out cand.iy1, ref InnerFailed);
                            LineScan(scanx, scany, -1, 0, (value) => { return newA - value > aTresh; }, (xvalue, yvalue) => { return xvalue < 0; }, out cand.ix0, out bordery, ref InnerFailed);
                            LineScan(scanx, scany, 1, 0, (value) => { return newA - value > aTresh; }, (xvalue, yvalue) => { return xvalue >= width; }, out cand.ix1, out bordery, ref InnerFailed);

                            cand.innerCenter = new Vector2((cand.ix0 + cand.ix1) / 2f, (cand.iy0 + cand.iy1) / 2f);
                                                       
                            //Debug.Log("valid ");

                            detections.Add(cand);

                            //debugPixels[cand.yCenterAsInt * width + cand.xCenterAsInt] = new Color(0, 0, 255);

                            // Build a polygon from the four scanpoints.

                            cand.InitialPolygon();
                            int polyCount = cand.polygonInner.Count;

                            //Vector2 center = new Vector2(cand.xCenter, cand.yCenter);

                            Vector2 center = cand.innerCenter;

                            Vector2 targetPoint = Vector2.zero;
                            Vector2 edgePoint = Vector2.zero;

                            int iterations = 0;
                            int p = 0;

                            // Build polygon. The polygon is always convex. 
                            // So for every three sections, the first and third intersect outside the polygon, which is the maximum point to which we can extend while remaining convex.
                            // We scan lines from the center to those target points.

                            while (iterations < 3)
                            {

                                // Sections are de-activated if a previous scan had no effect.

                                if (cand.active[p])

                                {

                                    // A2 A1 B1 B2
                                    // A1 B1 is the section we looking to scan through. A2 A1 and B2 B1 are the adjacent sections that define the target.

                                    Vector2 a2 = cand.polygonInner[(p - 1 + polyCount) % polyCount];
                                    Vector2 a1 = cand.polygonInner[p];

                                    Vector2 b2 = cand.polygonInner[(p + 2) % polyCount];
                                    Vector2 b1 = cand.polygonInner[(p + 1) % polyCount];

                                    if (!LineIntersection(a2, a1, b2, b1, ref targetPoint))
                                    {
                                        // If parallel we invent a point.

                                        targetPoint = 0.5f * (a1 + b1) + (a1 - a2);

                                    }

                                    // Special case. The target point can be (pretty much) on the A1 or B1 points which would result in an ineffective or faulty scan because of image resolution.
                                    // We adjust the target point a bit. Range is square root of step.

                                    float dx = Mathf.Abs(a1.x - targetPoint.x);
                                    dx = dx * dx;
                                    float dy = Mathf.Abs(a1.y - targetPoint.y);
                                    dy = dy * dy;

                                    if (dx + dy < step)
                                    {
                                        // target is on a1 point

                                        Vector2 adjust = 0.25f * (b1 - a1);
                                        targetPoint += adjust;
                                        Vector2.ClampMagnitude(adjust, step);

                                    }
                                    else
                                    {

                                        dx = Mathf.Abs(b1.x - targetPoint.x);
                                        dx = dx * dx;
                                        dy = Mathf.Abs(b1.y - targetPoint.y);
                                        dy = dy * dy;

                                        if (dx + dy < step)
                                        {
                                            // target is on b1 point

                                            Vector2 adjust = 0.25f * (a1 - b1);
                                            Vector2.ClampMagnitude(adjust, step);
                                            targetPoint += adjust;

                                        }

                                    }

                                    // Get an intersection between OT and A1 B1. This point is where the current polygon's edge is.

                                    if (!LineIntersection(center, targetPoint, a1, b1, ref edgePoint))
                                    {
                                        //edgePoint = targetPoint; // just to possibly prevent a crash but shouldn't happen.
                                        //Debug.LogError("cannot be parallel");
                                        cand.Rejected = true;
                                        break;
                                    }

                                    //  Detect a new point along center to target, potentially extending the polygon.

                                    Vector2 detectionPoint = ScanLine(center, targetPoint, newA);

                                    //debugPixels[Mathf.RoundToInt(detectionPoint.y) * width + Mathf.RoundToInt(detectionPoint.x)] = Color.cyan;
                                    //debugPixels[Mathf.RoundToInt(edgePoint.y) * width + Mathf.RoundToInt(edgePoint.x)] = Color.white;
                                    //debugPixels[Mathf.RoundToInt(targetPoint.y) * width + Mathf.RoundToInt(targetPoint.x)] = Color.black;

                                    // Special case: edgepoint and detectionpoint are almost equal.
                                    // Adding the new point wouldn't change the polygon. We lock this section to exclude it from subsequent scans.

                                    dx = Mathf.Abs(edgePoint.x - detectionPoint.x);
                                    dx = dx * dx;
                                    dy = Mathf.Abs(edgePoint.y - detectionPoint.y);
                                    dy = dy * dy;

                                    if (dx + dy < step)
                                    {
                                        cand.active[p] = false;

                                    }
                                    else
                                    {
                                        // Special case: the detection point is further inward. Either our detection is partially obscured or it is not marker.
                                        // To detect we check against a bounding box from center to edgepoint. If detection point is in it, it would make the polygon inconvex.

                                        dx = Mathf.Abs(edgePoint.x - center.x);
                                        dy = Mathf.Abs(edgePoint.y - center.y);

                                        if (Mathf.Abs(detectionPoint.x - edgePoint.x) < dx && Mathf.Abs(detectionPoint.x - center.x) < dx
                                            && Mathf.Abs(detectionPoint.y - edgePoint.y) < dy && Mathf.Abs(detectionPoint.y - center.y) < dy)
                                        {

                                            // Point is inward, meaning a deviation which we'll ignore to keep the polygon convex.

                                        }
                                        else
                                        {
                                            // Point is sufficiently beyond current edge to make for a meaningful expansion. 
                                            // Add it to the polygon.

                                            cand.polygonInner.Insert((p + 1), detectionPoint);
                                            cand.active.Insert((p + 1), true);
                                            polyCount++; // We're manually tracking the count.

                                        }

                                    }

                                }
                                else
                                {

                                    // Edge was locked. We don't do anything and move on.
                                    //Debug.Log("edge is locked, skipping");

                                }

                                p++;

                                if (p == polyCount)
                                {
                                    // If we've reached the end, we start over.

                                    p = 0;
                                    iterations++;

                                }


                            }

                            // Done building.


                            // Draw poly

                            for (int p2 = 0; p2 < cand.polygonInner.Count; p2++)
                            {
                                //debugTexture.SetPixels(debugPixels);
                                //debugTexture.Apply();
                                //Debug.Break();

                                //yield return new WaitForSeconds(0.25f);


                                Color col;

                                if (p2 % 2 == 0)
                                {
                                    col = Color.yellow;

                                }
                                else
                                {
                                    col = Color.magenta;

                                }

                                //DrawLine(cand.polygonInner[p2], cand.polygonInner[(p2 + 1) % cand.polygonInner.Count], debugPixels, col);


                            }



                            // Cluster the polygon sections into 4 groups which would constitute the sides of a potential quadrilateral.
                            // We use k means clustering.

                            // Create an array of headings.

                            float[] headings = new float[cand.polygonInner.Count];

                            for (int h = 0; h < headings.Length; h++)
                            {

                                Vector2 section = cand.polygonInner[(h + 1) % headings.Length] - cand.polygonInner[h];
                                headings[h] = Mathf.Atan2(section.y, section.x) * Mathf.Rad2Deg;

                                cand.AddToArea(cand.polygonInner[h]);
                                //debugPixels[Mathf.RoundToInt(cand.polygon[h].y) * width + Mathf.RoundToInt(cand.polygon[h].x)] = Color.cyan;
                            }

                            // Create initial cluster centers.

                            float[] centers = new float[4];
                            for (int c = 0; c < 4; c++)
                            {

                                centers[c] = headings[Mathf.RoundToInt(c * headings.Length / 4f)];
                                //Debug.Log("Center " + centers[c]);

                            }

                            // Assign values to closest cluster center, recalculate cluster centers, re-assign until no more changes.

                            KMEAN state = KMEAN.CHANGED;

                            int breakloop = 0;

                            int[] previousAssigments = new int[headings.Length];
                            previousAssigments[0] = -1;
                            previousAssigments[1] = -1;
                            previousAssigments[2] = -1;
                            previousAssigments[3] = -1;

                            int[] assigments = new int[headings.Length];

                            float squaredError = 0;

                            while (state == KMEAN.CHANGED && breakloop < 20)
                            {
                                breakloop++;
                                state = KMEAN.UNCHANGED;
                                squaredError = 0;

                                // Assign headings to nearest cluster.

                                assigments = new int[headings.Length];
                                float[] cumulatives = new float[4];
                                int[] members = new int[4];

                                for (int h = 0; h < headings.Length; h++)
                                {

                                    float closest = 99999;

                                    // Find nearest cluster center.

                                    for (int c = 0; c < 4; c++)
                                    {

                                        // We use delta angle to compare -178 to 178 and get delta 4.

                                        float distance = (Mathf.DeltaAngle(centers[c], headings[h]));

                                        //Debug.Log("angle " + distance + " between heading " + headings[h] + " and center " + centers[c]);

                                        if (Mathf.Abs(distance) < Mathf.Abs(closest))
                                        {
                                            assigments[h] = c;
                                            closest = distance;
                                        }

                                    }

                                    if (previousAssigments[h] != assigments[h])
                                    {
                                        state = KMEAN.CHANGED;
                                        previousAssigments[h] = assigments[h];
                                    }

                                    // Track cumulative squared error for all values.

                                    squaredError += closest * closest;

                                    // Track cumulative distances to adjust center.
                                    // Reason for this is that averageing angles gets complicated because of the -178 178 issue.
                                    // Instead we adjust by an averaged distance.

                                    cumulatives[assigments[h]] += closest;

                                    members[assigments[h]]++;

                                }

                                // Adjust cluster centers.

                                for (int c = 0; c < 4; c++)
                                {

                                    centers[c] = centers[c] + cumulatives[c] / members[c];

                                }

                                //Debug.Log("Error " + squaredError);

                            }

                            //Debug.Log("Error " + squaredError);

                            if (squaredError > MaxErrorSquared)
                            {
                                // Is not a quadrilateral but some other shape. We do keep it to prevent detecting it multiple times.
                                cand.Rejected = true;

                            }

                            // Now create the square from the clustered sections. Find corners first.

                            Vector2[] corners = new Vector2[4];
                            int corner = 0;

                            for (int a = 0; a < assigments.Length; a++)
                            {

                                if (assigments[a] != assigments[(a - 1 + assigments.Length) % assigments.Length])
                                {
                                    if (corner == 4)
                                    {
                                        //Debug.Log("reject");
                                        cand.Rejected = true; // Not a quadrilateral.
                                        break;
                                    }

                                    corners[corner] = cand.polygonInner[a];
                                    corner++;




                                }

                            }

                            //Debug.Log("corners found" + corners.Count);


                            if (!cand.Rejected)
                            {

                                // Find anchor by intersecting diagonal.s

                                Vector2 trackerAnchor = Vector2.zero;

                                if (!LineIntersection(corners[0], corners[2], corners[1], corners[3], ref trackerAnchor))
                                {
                                    Debug.LogError("cannot be parallel");
                                }

                                cand.anchor = trackerAnchor;

                                // Visual debug.

#if TRACKERDEBUG


                                Color marker = Color.cyan;

                                debugPixels[Mathf.RoundToInt(trackerAnchor.y) * width + Mathf.RoundToInt(trackerAnchor.x)] = Color.white;

                                Vector2 bbox00 = new Vector2(cand.xArea0, cand.yArea0);
                                Vector2 bbox01 = new Vector2(cand.xArea0, cand.yArea1);
                                Vector2 bbox10 = new Vector2(cand.xArea1, cand.yArea0);
                                Vector2 bbox11 = new Vector2(cand.xArea1, cand.yArea1);

                                DrawLine(bbox00, bbox01, debugPixels, Color.grey);
                                DrawLine(bbox01, bbox11, debugPixels, Color.grey);
                                DrawLine(bbox11, bbox10, debugPixels, Color.grey);
                                DrawLine(bbox10, bbox00, debugPixels, Color.grey);

                                //DrawLine(corners[0], corners[1], debugPixels, marker);
                                //DrawLine(corners[1], corners[2], debugPixels, marker);
                                //DrawLine(corners[2], corners[3], debugPixels, marker);
                                //DrawLine(corners[3], corners[0], debugPixels, marker);



#endif

                            }


                            // Below breaks loop.

                            //x = width - 1;
                            //y = height - 1;


                        }
                        //x = width - 1;
                        //y = height - 1;
                    }
                    oldA = newA;
                    //x = width - 1;
                    //y = height - 1;

                }



            }

#if TRACKERDEBUG
            // Visual debug.

            debugTexture.SetPixels(debugPixels);
            debugTexture.Apply();

#endif
            //Debug.Log("detections: " + detections.Count);


        }

        enum KMEAN
        {
            CHANGED,
            UNCHANGED
        }


        public bool LineIntersection(float[] line1, float[] line2, ref Vector2 intersection)

        {

            // a,b,c format.

            float a1 = line1[0];
            float b1 = line1[1];
            float c1 = line1[2];


            float a2 = line2[0];
            float b2 = line2[1];
            float c2 = line2[2];

            float det = a1 * b2 - a2 * b1;
            //Debug.Log("det " + det);

            if (det == 0)
            {
                //Lines are parallel, we invent a point

                Debug.Log("is parallel");

                //intersection = 0.5f * (p2 + p4) + (p2 - p1);




                return false;

                //return false;

            }
            else
            {
                intersection.x = (b2 * c1 - b1 * c2) / det;
                intersection.y = (a1 * c2 - a2 * c1) / det;
            }

            return true;


        }

        public bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, ref Vector2 intersection)


        {

            float a1 = p2.y - p1.y;
            float b1 = p1.x - p2.x;
            float c1 = a1 * p1.x + b1 * p1.y;


            float a2 = p4.y - p3.y;
            float b2 = p3.x - p4.x;
            float c2 = a2 * p4.x + b2 * p4.y;


            float det = a1 * b2 - a2 * b1;
            //Debug.Log("det " + det);

            if (det == 0)
            {
                //Lines are parallel, we invent a point

                //Debug.Log("is parallel");

                //intersection = 0.5f * (p2 + p4) + (p2 - p1);




                return false;

                //return false;

            }
            else
            {
                intersection.x = (b2 * c1 - b1 * c2) / det;
                intersection.y = (a1 * c2 - a2 * c1) / det;
            }

            return true;

        }






        void DrawLine(int x0, int y0, int x1, int y1, Color[] target, Color col)
        {



            DrawLine(new Vector2(x0, y0), new Vector2(x1, y1), target, col);






        }

        void DrawLine(Vector2 start, Vector2 end, Color[] target, Color col)
        {

            float ls = 1f / (end - start).magnitude;

            for (float t = 0; t <= 1; t += ls)
            {

                Vector2 point = Vector2.Lerp(start, end, t);

                target[(int)point.y * width + (int)point.x] = col;

            }





        }

        private bool IsInPreviousDets(int x, int y)
        {

            foreach (Detection det in detections)
            {

                if (det.isInArea(x, y))

                {
                    //Debug.Log("point already scanned");
                    return true;
                }

            }

            return false;

        }

        private bool IsInPreviousDets(ref int x, ref int y)
        {
            bool r = false;
            foreach (Detection det in detections)
            {

                if (det.isInArea(ref x, ref y))

                {
                    //Debug.Log("point already scanned");
                    r = true;
                }

            }

            return r;

        }



        Vector2 ScanLine(Vector2 start, Vector2 target, float refA)
        {

            int BorderDetection = 0;
            float scanA;
            //float tb = 0;
            float t = 0;

            float lstep = 1f / (target - start).magnitude;
            Vector2 BorderPoint = Vector2.zero;


            while (BorderDetection < outLimit)
            {
                Vector2 point = Vector2.LerpUnclamped(start, target, t);


                if (t >= 1f || point.x < 0 || point.x >= width - 0.55f || point.y < 0 || point.y >= height - 0.55f)
                {
                    //Debug.Log("scan beyond target");
                    // if we reach the target or canvas edge we abort, so that the polygon remains convex.

                    return BorderPoint;
                }

                scanA = LABColor.FromColor(pixels[Mathf.RoundToInt(point.y) * width + Mathf.RoundToInt(point.x)]).a;


                if (refA - scanA > aTresh)
                {
                    BorderDetection++;
                    //Debug.Log(LABColor.FromColor(pixels[ys * width + x]).ToString());
                }
                else
                {
                    BorderDetection = 0;
                    BorderPoint = point;
                }

                debugPixels[Mathf.RoundToInt(point.y) * width + Mathf.RoundToInt(point.x)] = Color.grey;

                t += lstep;




            }



            return BorderPoint;




        }

        delegate bool Condition(float value);
        delegate bool ErrorCondition(int x, int y);


        private float LineScan(int x, int y, int xmod, int ymod, Condition ScanCondition, ErrorCondition AbortCondition, out int borderx, out int bordery, ref bool failed)
        {

            int BorderDetection = 0;

            borderx = x;
            bordery = y;
            //bordera=0;
            float scanA = 0;

            while (BorderDetection < outLimit)
            {

                scanA = LABColor.FromColor(pixels[y * width + x]).a;
                //Debug.Log(scanA);

                if (ScanCondition(scanA))
                {
                    BorderDetection++;
                    //Debug.Log("detecting");
                }
                else
                {
                    BorderDetection = 0;
                    borderx = x;
                    bordery = y;

                }

                debugPixels[y * width + x] = new Color(0, 0, 0);

                x += xmod;
                y += ymod;

                if (AbortCondition(x, y))
                {
                    failed = true;
                    break;
                    //return false;
                }
            }
            return scanA;
            //bordera=scanA;

            //return true;
        }

        //private Detection OrthScan(float x, float y, float refA, Detection previousDet)

        //{

        //    return OrthScan(Mathf.RoundToInt(x), Mathf.RoundToInt(y), refA, previousDet);
        //}

        private void OrthScan(int x, int y, float refA, ref Detection det)
        {

            //Detection det = new Detection();

            // A scan is invalid if it leaves the canvas.

            //det.isValid = true;

            // A scan is converging if the squared distance from the ref point to the resulting center is smaller than in the previous scan.

            float oldConvergenceInner = det.convergenceInner;
            float oldConvergenceOuter = det.convergenceOuter;

            //int ix0=0,ix1=0,iy0=0,iy1=0,ox0=0,ox1=0,oy0=0,oy1=0;

            // Find inner and outer border points.

            int borderx, bordery;
            float outa;

            bool InnerFailed = false;
            bool OuterFailed = false;

            outa = LineScan(x, y, 0, -step, (value) => { return refA - value > aTresh; }, (xvalue, yvalue) => { return yvalue < 0; }, out borderx, out det.iy0, ref InnerFailed);
            LineScan(x, det.iy0, 0, -step, (value) => { return (value - outa) > aTresh / 2; }, (xvalue, yvalue) => { return yvalue < 0; }, out borderx, out det.oy0, ref OuterFailed);

            outa = LineScan(x, y, 0, step, (value) => { return refA - value > aTresh; }, (xvalue, yvalue) => { return yvalue >= height; }, out borderx, out det.iy1, ref InnerFailed);
            LineScan(x, det.iy1, 0, step, (value) => { return (value - outa) > aTresh / 2; }, (xvalue, yvalue) => { return yvalue >= height; }, out borderx, out det.oy1, ref OuterFailed);

            outa = LineScan(x, y, -step, 0, (value) => { return refA - value > aTresh; }, (xvalue, yvalue) => { return xvalue < 0; }, out det.ix0, out bordery, ref InnerFailed);
            LineScan(det.ix0, y, -step, 0, (value) => { return (value - outa) > aTresh / 2; }, (xvalue, yvalue) => { return xvalue < 0; }, out det.ox0, out bordery, ref OuterFailed);

            outa = LineScan(x, y, step, 0, (value) => { return refA - value > aTresh; }, (xvalue, yvalue) => { return xvalue >= width; }, out det.ix1, out bordery, ref InnerFailed);
            LineScan(det.ix1, y, step, 0, (value) => { return (value - outa) > aTresh / 2; }, (xvalue, yvalue) => { return xvalue >= width; }, out det.ox1, out bordery, ref OuterFailed);

            // calculate new centers

            det.innerCenter = new Vector2((det.ix0 + det.ix1) / 2f, (det.iy0 + det.iy1) / 2f);
            det.outerCenter = new Vector2((det.ox0 + det.ox1) / 2f, (det.oy0 + det.oy1) / 2f);

            // calculate new convergence

            det.convergenceInner = (det.innerCenter.x - x) * (det.innerCenter.x - x) + (det.innerCenter.y - y) * (det.innerCenter.y - y);
            det.convergenceOuter = (det.outerCenter.x - x) * (det.outerCenter.x - x) + (det.outerCenter.y - y) * (det.outerCenter.y - y);

            det.convergingInner = (det.convergenceInner < oldConvergenceInner);
            det.convergingOuter = (det.convergenceOuter < oldConvergenceOuter);

            //Debug.Log(det.convergenceInner);
            //Debug.Log(det.convergingOuter);





            det.isValid = !(InnerFailed || OuterFailed) && (det.ix1 - det.ix0) > step * 2 && (det.iy1 - det.iy0) > step * 2;

            //Debug.Log(det.isValid);

            //debugPixels[y * width + xb] = new Color(255, 0, 255);

            // Convergence is the squared distance from the passed in x,y to the resulting center of the scan. 

            //det.convergenceInner = (det.xCenter - x) * (det.xCenter - x) + (det.yCenter - y) * (det.yCenter - y);

            //det.convergingInner = false;

            //if (det.convergenceInner < previousDet.convergenceInner)
            //det.convergingInner = true;


            //return det;


        }

    }

    public class Detection
    {

        public int ix0, ix1, iy0, iy1;
        public int ox0, ox1, oy0, oy1;

        public Vector2 innerCenter, outerCenter;

        public int xArea0, xArea1, yArea0, yArea1;

        public bool isValid, convergingInner, convergingOuter;

        public float convergenceInner, convergenceOuter;

        public bool Rejected;

        public Vector2 anchor,lastAnchor;

        public List<Vector2> polygonInner;

        public List<bool> active;

        public bool tracking;

        public void ApplyFrom (Detection detection){


            xArea0=detection.xArea0;
            xArea1=detection.xArea1;
            yArea0=detection.yArea0;
            yArea1=detection.yArea1;
            lastAnchor=anchor;
            anchor=detection.anchor;

            tracking=true;
        }

        public bool IsInSearchArea (Detection detection){

            // is this in detection's search area?
        
            int x0= Mathf.Min(xArea0,detection.xArea0);
            int x1= Mathf.Max(xArea1,detection.xArea1);
            int y0= Mathf.Min(yArea0,detection.yArea0);
            int y1= Mathf.Max(yArea1,detection.yArea1);

            if (x1-x0 < xArea1-xArea0 + detection.xArea1-xArea0 && y1-y0 < yArea1-yArea0 + detection.yArea1-yArea0)
                return true;

            return false;


        }

        public Detection()
        {

            xArea0 = 9999;
            yArea0 = 9999;
            xArea1 = -9999;
            yArea1 = -9999;
            convergenceInner = 99999999999999;
            convergenceOuter = 99999999999999;
            Rejected = false;
            tracking=false;
            lastAnchor=Vector2.zero;
        }

        public void AddToArea(Vector2 point)
        {

            xArea0 = (int)Mathf.Min(xArea0, point.x);
            xArea1 = (int)(Mathf.Max(xArea1, point.x));

            yArea0 = (int)Mathf.Min(yArea0, point.y);
            yArea1 = (int)(Mathf.Max(yArea1, point.y));

        }

        public void InitialPolygon()
        {

            polygonInner = new List<Vector2>();
            polygonInner.Add(new Vector2(ix0, innerCenter.y));
            polygonInner.Add(new Vector2(innerCenter.x, iy0));
            polygonInner.Add(new Vector2(ix1, innerCenter.y));
            polygonInner.Add(new Vector2(innerCenter.x, iy1));

            active = new List<bool>();
            active.Add(true);
            active.Add(true);
            active.Add(true);
            active.Add(true);

        }

        public bool isInArea(int x, int y)
        {

            return (x >= xArea0 && x <= xArea1 && y >= yArea0 && y <= yArea1);

        }

        public bool isInArea(ref int x, ref int y)
        {

            if (  xArea0 == 9999){
                Debug.LogError("area not set");
            }
            if (x >= xArea0 && x <= xArea1 && y >= yArea0 && y <= yArea1)
            {
                // is in area, we move the x value on
                x = xArea1;
                return true;
            }

            return false;

        }

        /*
        public int xCenterAsInt
        {

            get
            {
                return Mathf.RoundToInt(xCenter);
            }

        }
        public int yCenterAsInt
        {

            get
            {
                return Mathf.RoundToInt(yCenter);
            }

        }
        public float xCenter
        {

            get
            {

                return (ix0 + ix1) / 2f;
            }

        }

        public float yCenter
        {

            get
            {

                return (iy0 + iy1) / 2f;
            }

        }
        */

    }

}
