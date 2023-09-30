using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

namespace Utility
{
    [RequireComponent(typeof(SplineContainer))]
    public class SplineTerrainBrush : MonoBehaviour
    {
        [SerializeField] private Terrain terrain;
        [SerializeField] private Texture2D brush;
        [FormerlySerializedAs("width")] [SerializeField] private float radius = 1;
        [SerializeField] private float strength = 1;
        [SerializeField] private TerrainLayer layer;
        [SerializeField, HideInInspector] private string guid;

        private UndoBufferData _undoBuffer;
        private float[,,] _terrainWeights;

        [Serializable]
        public class Int2
        {
            public Int2(int x, int y)
            {
                X = x;
                Y = y;
            }
            public int X;
            public int Y;
        }
        [Serializable]
        public class UndoBufferData
        {
            public List<Tuple<Int2, float[,,]>> UndoData = new();
        }

        private void LoadUndoBuffer()
        {
            if (File.Exists($"UndoBuffer/{guid}.dat") == false)
            {
                _undoBuffer = new();
            }
            else
            {
                var fs = new FileStream($"UndoBuffer/{guid}.dat", FileMode.Open);
                var formatter = new BinaryFormatter();
                _undoBuffer = (UndoBufferData)formatter.Deserialize(fs);
                fs.Close();
            }
        }

        private void SaveUndoBuffer()
        {
            using (var fs = new FileStream($"UndoBuffer/{guid}.dat", FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fs, _undoBuffer);
            }
            
            _undoBuffer.UndoData.Clear();
        }

        [Button("Apply")]
        public void Apply()
        {
            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString();
            }
            var terrainData = terrain.terrainData;
            _terrainWeights = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

            Undo(false);

            var undo = new List<Tuple<Int2, float[,,]>>();
            _undoBuffer.UndoData = undo;
            
            
            int layerIndex = -1;
            for (int i = 0; i < terrainData.terrainLayers.Length; i++)
            {
                if (layer == terrainData.terrainLayers[i])
                {
                    layerIndex = i;
                    break;
                }
            }

            if (layerIndex == -1)
            {
                Debug.LogError("Layer not found on terrain");
            }
            
            var spline = GetComponent<SplineContainer>();
            var length = spline.CalculateLength();
            var resizedBrush = GetResizedBrush((int)radius, (int)radius);
            for (float t = 0; t < 1; t += 1 / length)
            {
                var worldPoint = spline.EvaluatePosition(t);
                Paint(worldPoint, layerIndex, resizedBrush);
            }
            
            ApplyChanges();
            DestroyImmediate(resizedBrush);

            SaveUndoBuffer();
            _undoBuffer.UndoData.Clear();
        }

        private void SetAlphaMap(int x, int y, float[,,] map)
        {
            int width = map.GetLength(1);
            int height = map.GetLength(0);
            int weights = map.GetLength(2);
            for (int xx = 0; xx < width; xx++)
            {
                for (int yy = 0; yy < height; yy++)
                {
                    for (int zz = 0; zz < weights; zz++)
                    {
                        _terrainWeights[y + yy, x + xx, zz] = map[yy, xx, zz];
                    }
                }
            }
        }

        private float[,,] GetAlphaMap(int x, int y, int width, int height)
        {
            var layers = terrain.terrainData.alphamapLayers;
            var result = new float[width, height, layers];

            for (int xx = 0; xx < width; xx++)
            {
                for (int yy = 0; yy < height; yy++)
                {
                    for (int zz = 0; zz < layers; zz++)
                    {
                        result[yy, xx, zz] = _terrainWeights[y + yy, x + xx, zz];
                    }
                }
            }

            return result;
        }

        private void ApplyChanges()
        {
            terrain.terrainData.SetAlphamaps(0, 0, _terrainWeights);
            terrain.Flush();
        }
        
        [Button("Undo")]
        public void Undo(bool apply = true)
        {
            var terrainData = terrain.terrainData;
            if (apply)
            {
                _terrainWeights = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            }
            LoadUndoBuffer();

            if (_undoBuffer.UndoData == null) return;

            var undoData = _undoBuffer.UndoData;
            
            for (int i = undoData.Count - 1; i >= 0; i--) // Apply in reverse
            {
                var data = undoData[i];
                SetAlphaMap(data.Item1.X, data.Item1.Y, data.Item2);
            }

            if (apply)
            {
                ApplyChanges();
            }

            _undoBuffer.UndoData.Clear();

            SaveUndoBuffer();
        }

        [Button("Generate GUID")]
        public void NewGuid()
        {
            guid = Guid.NewGuid().ToString();
        }

        private Texture2D GetResizedBrush(int width, int height)
        {
            var rt = new RenderTexture(width, height, 24);
            RenderTexture.active = rt;
            Graphics.Blit(brush, rt);
            
            var result=new Texture2D(width,height);
            result.ReadPixels(new Rect(0,0,width,height),0,0);
            result.Apply();
            DestroyImmediate(rt);
            return result;
        }

        private void AddUndoPoint(int x, int y, int width, int height)
        {
            var splat = GetAlphaMap(x, 
                y, 
                width, 
                height);

            var undoData = new Tuple<Int2, float[,,]>(new(x, y), splat);
            _undoBuffer.UndoData.Add(undoData);
        }
        private void Paint(Vector3 worldPoint, int layerIndex, Texture2D resizedBrush)
        {
            var point = GetTerrainCoordinates(worldPoint);
            int xMod = 0; // Modifier for when we paint at the terrain's edge
            int yMod = 0;// Modifier for when we paint at the terrain's edge
            int widthMod = 0;
            int heightMod = 0;
            
            var terrainData = terrain.terrainData;
            
            if (point.x < 0) // if the brush goes off the negative end of the x axis we set the mod == to it to offset the edited area
            {
                xMod = Mathf.FloorToInt(point.x);
            }
            else if (point.x + radius  > terrainData.alphamapWidth)// if the brush goes off the posative end of the x axis we set the mod == to this
            {
                widthMod = Mathf.FloorToInt(point.x + radius - terrainData.alphamapWidth);
            }
    
            if (point.y < 0)//same as with x
            {
                yMod = Mathf.FloorToInt(point.y);
            }
            else if (point.y + radius > terrainData.alphamapHeight)
            {
                heightMod = Mathf.FloorToInt(point.y + radius - terrainData.alphamapHeight);
            }

            int x = Mathf.FloorToInt(point.x - xMod);
            int y = Mathf.FloorToInt(point.y - yMod);
            int width = Mathf.FloorToInt(radius + widthMod);
            int height = Mathf.FloorToInt(radius + heightMod);
            
            AddUndoPoint(x, y, width, height);
            
            var splat = GetAlphaMap(x, y, width, height); //grabs the splat map data for our brush area

            for (int xx = 0; xx < radius + widthMod; xx++)
            {
                for (int yy = 0; yy < radius + heightMod; yy++)
                {
                    float[] weights = new float[terrainData.alphamapLayers]; //creates a float array and sets the size to be the number of paints your terrain has
                    for (int zz = 0; zz < splat.GetLength(2); zz++)
                    {
                        weights[zz] = splat[xx, yy, zz];//grabs the weights from the terrains splat map
                    }

                    var pixel = resizedBrush.GetPixel(xx, yy).a;
                    weights[layerIndex] += pixel * strength; // adds weight to the paint currently selected with the int paint variable
                    
                    //this next bit normalizes all the weights so that they will add up to 1
                    float sum = weights.Sum();
                    for (int ww = 0; ww < weights.Length; ww++)
                    {
                        weights[ww] /= sum;
                        splat[xx, yy, ww] = weights[ww];
                    }
                    
                }
            }
            
            SetAlphaMap(Mathf.FloorToInt(point.x - xMod), Mathf.FloorToInt(point.y - yMod), splat);
        }
        
        private Vector2 GetTerrainCoordinates(Vector3 worldPoint)
        {
            var offset = radius / 2; //This offsets the hit position to account for the size of the brush which gets drawn from the corner out
            //World Position Offset Coords, these can differ from the terrain coords if the terrain object is not at (0,0,0)
            Vector3 tempTerrainCoodinates = worldPoint - terrain.transform.position;
            //This takes the world coords and makes them relative to the terrain
            Vector3 terrainCoordinates = new Vector3(
                tempTerrainCoodinates.x / GetTerrainSize().x,
                tempTerrainCoodinates.y / GetTerrainSize().y,
                tempTerrainCoodinates.z / GetTerrainSize().z);

            // This will take the coords relative to the terrain and make them relative to the height map(which often has different dimensions)
            var terrainData = terrain.terrainData;
            Vector3 locationInTerrain = new Vector3
            (
                terrainCoordinates.x * terrainData.alphamapWidth,
                0,
                terrainCoordinates.z * terrainData.alphamapHeight
            );

            return new(locationInTerrain.x - offset, locationInTerrain.z - offset);
        }
        
        public Vector3 GetTerrainSize()
        {
            return terrain.terrainData.size;
        }

    }
}