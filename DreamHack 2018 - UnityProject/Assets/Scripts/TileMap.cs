﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.TileObjects;
using System.Xml.Linq;
using System;

namespace Game
{
    public class TileMap
    {
        protected Tile[,] m_tiles;

        protected GameObject m_mapObject;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public TileMap(int width, int height)
        {
            Width = width;
            Height = height;
            m_tiles = new Tile[Width, Height];

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    m_tiles[x, y] = new Tile(this, x, y);
                }
            }
        }

        public void Parse(XElement element)
        {
            if (element == null)
                return;

            XAttribute widthAttrib = element.Attribute("width");
            XAttribute heightAttrib = element.Attribute("height");

            IEnumerable<XElement> tileElements = element.Elements("Tile");
            foreach(XElement tileElement in tileElements)
            {
                Tile tile = Tile.CreateAndParse(tileElement, this);
                if (tile != null)
                    m_tiles[tile.Position.X, tile.Position.Y] = tile;
            }

            if(widthAttrib != null)
                Width = Int32.Parse(widthAttrib.Value);
            if (heightAttrib != null)
                Height = Int32.Parse(heightAttrib.Value);
        }

        public void Populate(XElement element)
        {
            element.SetAttributeValue("width", Width);
            element.SetAttributeValue("height", Height);

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    if (m_tiles[x, y] != null)
                    {
                        XElement tileElement = new XElement("Tile");
                        element.Add(tileElement);
                        m_tiles[x, y].Populate(tileElement);
                    }
                }
            }
        }

        public GameObject CreateGameObject()
        {
            if(m_mapObject != null)
            {
                UnityEngine.Object.Destroy(m_mapObject);
            }

            m_mapObject = new GameObject("TileMap");
            m_mapObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            for(int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    if (m_tiles[x, y] != null)
                    {
                        GameObject tileObject = UnityEngine.Object.Instantiate(WorldController.Instance.TilePrefab, m_mapObject.transform);
                        tileObject.name = "Tile (" + x + "," + y + ")";
                        tileObject.transform.SetParent(m_mapObject.transform);
                        tileObject.transform.SetPositionAndRotation(new Vector3(x, 0, y), Quaternion.identity);
                    }
                }
            }

            return m_mapObject;
        }

        public Tile TileAt(TilePosition position)
        {
            if (position.X < 0 || position.X >= Width ||
                position.Y < 0 || position.Y >= Height)
                return null;

            return m_tiles[position.X, position.Y];
        }

        public bool InstallAt(TileObjectBase objectToInstall, TilePosition targetPosition)
        {
            Tile targetTile = TileAt(targetPosition);
            if (targetTile == null)
                return false;

            targetTile.Install(objectToInstall);
            return true;
        }
    }
}