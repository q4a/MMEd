using System;
using System.Collections.Generic;
using System.Text;

namespace GLTK
{
  public class Scene
  {
    public virtual void AddRange(IEnumerable<Entity> xiCollection)
    {
        mObjects.AddRange(xiCollection);
    }

    public virtual void AddObject(Entity xiObject)
    {
      if (!mObjects.Contains(xiObject))
      {
        mObjects.Add(xiObject);
      }
    }

    public virtual void RemoveObject(Entity xiObject)
    {
      if (mObjects.Contains(xiObject))
      {
        mObjects.Remove(xiObject);
      }
    }

    public void AddLight(Light xiLight)
    {
      mLights.Add(xiLight);
    }

    public void RemoveLight(Light xiLight)
    {
      if (mLights.Contains(xiLight))
      {
        mLights.Remove(xiLight);
      }
    }

    public void Clear()
    {
        mObjects.Clear();
    }

    public int ObjectCount
    {
      get
      {
        return mObjects.Count;
      }
    }

    public Entity this[int xiIndex]
    {
      get
      {
        return mObjects[xiIndex];
      }
    }

    public List<Entity> Objects
    {
      get
      {
        return mObjects;
      }
    }

    public IEnumerable<Entity> Entities
    {
      get
      {
        return mObjects;
      }
    }

    internal List<Light> Lights
    {
      get
      {
        return mLights;
      }
    }

    private List<Light> mLights = new List<Light>();
    private List<Entity> mObjects = new List<Entity>();
  }
}
