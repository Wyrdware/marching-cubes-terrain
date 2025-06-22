using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


//The Scalar Field is only viable for positive values.

public class ScalarField : MonoBehaviour
{
    // Start is called before the first frame update
    private Dictionary<Vector3Int, FieldValue> field = new();
    [SerializeField]
    private float metersPerUnit= 1f; 
    
    private ScalarPopulateStrategy populateStrategy;

    private void Awake()
    {
        populateStrategy = new FlowFieldPopulateStrategy();
        //populateStrategy = new PerlinPopulateStrategy();
    }


    public void TestPopulateRange()
    {
        float range= 20;
        for (int x = 0; x < range; x++)
        {
            for (int y = 0; y < range; y++)
            {
                for (int z = 0; z < range; z++)
                {

                    Vector3 position = new Vector3(x, y, z) * metersPerUnit;
                    float value = Sample(position);
                    Debug.Log("Position: " + position + "Value: " + value);
                }
            }        
        }

    }
    public float Sample(Vector3 position)//Position must be positivew
    {
        Vector3Int gridPosition = Vector3Int.FloorToInt(position / metersPerUnit);

        if (!field.ContainsKey(gridPosition))
        {
            field.Add(gridPosition, new FieldValue(populateStrategy.Populate(position)));
        }

        return field[gridPosition].getValue();
    }
    public float GetMetersPerUnit()
    {
        return metersPerUnit;
    }

    private class FieldValue
    {
        float baseValue = 0f;
        float modifier = 0f;
        public FieldValue(float baseValue)
        {
            this.baseValue = Mathf.Clamp01(baseValue);
        }

        public void Modify(float value)
        {
            float newModifier = Mathf.Clamp01(baseValue + (modifier+value)) -baseValue;
            modifier = newModifier;
        }
        public float getValue()
        {
            return baseValue + modifier;
        }
    }

    
    private void OnDrawGizmosSelected()
    {
        if (field != null)
        {
            foreach (KeyValuePair<Vector3Int, FieldValue> entry in field)
            {
                Color HighDensity = (new Color(1, 1, 1, 1));
                Color LowDensity = new Color(1, 0.5f, 0.5f, 0.1f);
                Gizmos.color = Color.Lerp(LowDensity, HighDensity, entry.Value.getValue());
                Gizmos.DrawCube(metersPerUnit * (Vector3)entry.Key, Vector3.one * metersPerUnit * 0.5f);
            }
        }
    }
}

public abstract class ScalarPopulateStrategy
{
    public abstract float Populate(Vector3 point);
}

public class FlowFieldPopulateStrategy : ScalarPopulateStrategy
{
    public override float Populate(Vector3 point) 
    {
        float density;
        Vector3 warpedPoint = point;
        float offsetRate;

        Vector3 offsetPoint = new Vector3(point.x, point.y, point.z);
        offsetRate = (Perlin.EvaluateFBM(offsetPoint, 1f, 0.005f, 4, .9f, 0.1f) + 1) * 0.5f;//
        offsetRate = 1;
    
        //warpedPoint += Perlin.FlowField(point * 0.005f *offsetRate) * 100f;
        warpedPoint += Perlin.FlowField(point * 0.05f * offsetRate) * 30f;
        //warpedPoint += Perlin.FlowField((point+Vector3.one*99) * 0.1f * offsetRate) * 10f;
        //warpedPoint += Perlin.FlowField(point* 0.15f) * 5f;

        float warpWeight = (Perlin.Noise(new Vector2(point.x+1000,point.z+1000)*0.004f)  + 1) * 0.5f;
        warpWeight *= 3;
        warpWeight -= 1;
        warpWeight = Mathf.SmoothStep(0, 1, warpWeight);
        warpedPoint = Vector3.Lerp(point, warpedPoint, warpWeight);
        density = -warpedPoint.y*0.3f;

        //density += (Perlin.EvaluateFBM(point*.3f, 0.5f, 0.5f, 4, .5f, 2f) + 1) * 2f ;
        density += (Perlin.EvaluateFBM(point , 5f, 0.005f, 5, .9f, 0.1f) + 1)*2;
 
        

        if (point.y <= -1f)
        {
            //density += 40;
        }



        return density;
    }
}
public class PerlinPopulateStrategy : ScalarPopulateStrategy
{
    public override float Populate(Vector3 point)
    {
        float density;

        float offsetRate = 1;
        offsetRate = (Perlin.EvaluateFBM(point, 1f, 0.001f, 4, .9f, 0.1f) + 1) * 0.5f;
        //Debug.Log(offsetRate);

        //warpedPoint += Perlin.FlowField(point * 0.15f) * 5f;
        density = -point.y * 0.3f;

        //density += (Perlin.EvaluateFBM(point*.3f, 0.5f, 0.5f, 4, .5f, 2f) + 1) * 2f ;
        density += (Perlin.EvaluateFBM(point, 1f, 0.005f, 4, .9f, 0.1f) + 1) * 0.5f *10;

        if (point.y <= -1f)
        {
            density += 40;
        }



        return density;
    }
}



