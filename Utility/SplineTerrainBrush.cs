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
        [SerializeField] private Texture2D brush;
        [FormerlySerializedAs("width")] [SerializeField] private float radius = 1;
        [SerializeField] private float strength = 1;
        [SerializeField] private TerrainLayer layer;
        [SerializeField, HideInInspector] private string guid;
        [SerializeField] private bool limitHeight;
        [SerializeField] private float heightSoftness;

        private UndoBufferData _undoBuffer;

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
            public Dictionary<string, List<Tuple<Int2, float[,,]>>> UndoData = new();
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
            LoadUndoBuffer();
            var terrains = FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            foreach (var terrain in terrains)
            {
                Apply(terrain);
            }
            
            SaveUndoBuffer();
        }
        
        private void Apply(Terrain terrain)
        {
            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString();
            }
            var terrainData = terrain.terrainData;
            var terrainWeights = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

            Undo(terrain, terrainWeights, false);

            var terrainExtension = terrain.GetComponent<TerrainExtension>();
            if (terrainExtension == null)
            {
                terrainExtension = terrain.gameObject.AddComponent<TerrainExtension>();
                terrainExtension.Initialize();
            }

            var terrainGuid = terrainExtension.guid;
            var undo = new List<Tuple<Int2, float[,,]>>();
            _undoBuffer.UndoData[terrainGuid] = undo;
            
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
                return; // Layer wasn't present on terrain
            }
            
            var splineContainer = GetComponent<SplineContainer>();
           
            var resizedBrush = GetResizedBrush((int)radius, (int)radius);
            foreach (var spline in splineContainer.Splines)
            {
                var length = spline.GetLength();
                for (float t = 0; t < 1; t += 1 / length)
                {
                    var worldPoint = (Vector3)(spline.EvaluatePosition(t)) + splineContainer.transform.position;
                    Paint(worldPoint, layerIndex, resizedBrush, terrain, terrainWeights, terrainGuid);
                }
            }

            ApplyChanges(terrain, terrainWeights);
            DestroyImmediate(resizedBrush);
        }
        

        private void ApplyChanges(Terrain terrain, float[,,] terrainWeights)
        {
            terrain.terrainData.SetAlphamaps(0, 0, terrainWeights);
            terrain.Flush();
        }

        [Button("Undo")]
        public void Undo()
        {
            LoadUndoBuffer();
            var terrains = FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            foreach (var terrain in terrains)
            {
                Undo(terrain, null, true);
            }
            SaveUndoBuffer();
        }
        
        public void Undo(Terrain terrain, float[,,] terrainWeights, bool apply)
        {
            var terrainData = terrain.terrainData;
            if (apply)
            {
                terrainWeights = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
            }

            if (_undoBuffer.UndoData == null) return;

            var terrainExtension = terrain.GetComponent<TerrainExtension>();
            if (terrainExtension == null || _undoBuffer.UndoData.ContainsKey(terrainExtension.guid) == false) return;
            
            var undoData = _undoBuffer.UndoData[terrainExtension.guid];
            
            
            for (int i = undoData.Count - 1; i >= 0; i--) // Apply in reverse
            {
                var data = undoData[i];
                SetAlphaMap(data.Item1.X, data.Item1.Y, data.Item2, terrainWeights);
            }

            if (apply)
            {
                ApplyChanges(terrain, terrainWeights);
            }
        }

        private Texture2D GetResizedBrush(int width, int height)
        {
            var rt = new RenderTexture(width, height, 24);
            RenderTexture.active = rt;
            Graphics.Blit(brush, rt);
            
            var result=new Texture2D(width,height);
            result.ReadPixels(new Rect(0,0,width,height),0,0);
            result.Apply();
            RenderTexture.active = null;
            DestroyImmediate(rt);
            return result;
        }

        private void AddUndoPoint(int x, int y, int width, int height, float[,,] terrainWeights, string terrainGuid)
        {
            var terrainWidth = terrainWeights.GetLength(1);
            var terrainHeight = terrainWeights.GetLength(0);

            if (x >= terrainWidth || x + width < 0 ||
                y >= terrainHeight || y + height < 0)
            {
                return;
            }
            
            var splat = GetAlphaMap(x, y, width, height, terrainWeights);

            var undoData = new Tuple<Int2, float[,,]>(new(x, y), splat);
            _undoBuffer.UndoData[terrainGuid].Add(undoData);
        }
        private void Paint(Vector3 worldPoint, int layerIndex, Texture2D resizedBrush, Terrain terrain, float[,,] terrainWeights, string terrainGuid)
        {
            var point = GetTerrainCoordinates(worldPoint, terrain);
            var terrainData = terrain.terrainData;

            if (point.x + radius < 0 || point.x >= terrainData.alphamapWidth ||
                point.y + radius < 0 || point.y >= terrainData.alphamapHeight)
            {
                return;
            }

            

            int x = Mathf.FloorToInt(point.x);
            int y = Mathf.FloorToInt(point.y);
            int width = Mathf.FloorToInt(radius);
            int height = Mathf.FloorToInt(radius);
            
            AddUndoPoint(x, y, width, height, terrainWeights, terrainGuid);
            
            var splat = GetAlphaMap(x, y, width, height, terrainWeights); //grabs the splat map data for our brush area

            for (int xx = 0; xx < radius; xx++)
            {
                for (int yy = 0; yy < radius; yy++)
                {
                    float hardnessModifier = 1;
                    if (limitHeight) // Don't paint above spline position if this is enabled
                    {
                        var pixelToWorldWidth = terrainData.size.x / terrainData.alphamapWidth;
                        var pixelToWorldHeight = terrainData.size.z / terrainData.alphamapHeight;
                        var samplePosition = new Vector3(
                            worldPoint.x + (float) (yy - radius / 2) * pixelToWorldWidth, // x and y are inverted on the splat array
                            worldPoint.y,
                            worldPoint.z + (float) (xx - radius / 2) * pixelToWorldHeight);
                        var terrainHeight = terrain.SampleHeight(samplePosition);
                        if (worldPoint.y < terrainHeight)
                        {
                            continue;
                        }
                        else if (worldPoint.y - heightSoftness < terrainHeight)
                        {
                            hardnessModifier = 1 - Mathf.InverseLerp(worldPoint.y - heightSoftness, worldPoint.y, terrainHeight);
                        }
                    }
                    float[] weights = new float[terrainData.alphamapLayers]; //creates a float array and sets the size to be the number of paints your terrain has
                    for (int zz = 0; zz < splat.GetLength(2); zz++)
                    {
                        weights[zz] = splat[xx, yy, zz];//grabs the weights from the terrains splat map
                    }

                    var pixel = resizedBrush.GetPixel(xx, yy).a;

                    weights[layerIndex] +=
                        pixel * strength * hardnessModifier; // adds weight to the paint currently selected with the int paint variable
                    

                    //this next bit normalizes all the weights so that they will add up to 1
                    float sum = weights.Sum();
                    for (int ww = 0; ww < weights.Length; ww++)
                    {
                        weights[ww] /= sum;
                        splat[xx, yy, ww] = weights[ww];
                    }
                    
                }
            }
            
            SetAlphaMap(Mathf.FloorToInt(point.x), Mathf.FloorToInt(point.y), splat, terrainWeights);
        }
        
        private Vector2 GetTerrainCoordinates(Vector3 worldPoint, Terrain terrain)
        {
            var offset = radius / 2; //This offsets the hit position to account for the size of the brush which gets drawn from the corner out
            //World Position Offset Coords, these can differ from the terrain coords if the terrain object is not at (0,0,0)
            Vector3 tempTerrainCoodinates = worldPoint - terrain.transform.position;
            //This takes the world coords and makes them relative to the terrain
            var data = terrain.terrainData;
            Vector3 terrainCoordinates = new Vector3(
                tempTerrainCoodinates.x / data.size.x,
                tempTerrainCoodinates.y / data.size.y,
                tempTerrainCoodinates.z / data.size.z);

            // This will take the coords relative to the terrain and make them relative to the height map(which often has different dimensions)
            var terrainData = data;
            Vector3 locationInTerrain = new Vector3
            (
                terrainCoordinates.x * terrainData.alphamapWidth,
                0,
                terrainCoordinates.z * terrainData.alphamapHeight
            );

            return new(locationInTerrain.x - offset, locationInTerrain.z - offset);
        }

        private void SetAlphaMap(int x, int y, float[,,] map, float[,,] terrainWeights)
        {
            int width = map.GetLength(1);
            int height = map.GetLength(0);
            int weights = map.GetLength(2);
            
            int terrainWidth = terrainWeights.GetLength(1);
            int terrainHeight = terrainWeights.GetLength(0);
            int terrainWeightCount = terrainWeights.GetLength(2);
            
            for (int xx = 0; xx < width; xx++)
            {
                if (xx + x>= terrainWidth || xx + x< 0) continue;
                for (int yy = 0; yy < height; yy++)
                {
                    if (yy + y >= terrainHeight || yy + y < 0) continue;
                    for (int zz = 0; zz < weights; zz++)
                    {
                        if (zz >= weights || zz >= terrainWeightCount) continue;
                        terrainWeights[y + yy, x + xx, zz] = map[yy, xx, zz];
                    }
                }
            }
        }

        private float[,,] GetAlphaMap(int x, int y, int width, int height, float[,,] terrainWeights)
        {
            var terrainWidth = terrainWeights.GetLength(1);
            var terrainHeight = terrainWeights.GetLength(0);
            var layers = terrainWeights.GetLength(2);
            
            var result = new float[width, height, layers];

            for (int xx = 0; xx < width; xx++)
            {
                if (x + xx >= terrainWidth || x + xx < 0) continue;
                for (int yy = 0; yy < height; yy++)
                {
                    if (y + yy >= terrainHeight || y + yy < 0) continue;
                    for (int zz = 0; zz < layers; zz++)
                    {
                        result[yy, xx, zz] = terrainWeights[y + yy, x + xx, zz];
                    }
                }
            }

            return result;
        }
    }
}