namespace UnityX.MeshBuilder {
    [System.Serializable]
    public struct MeshBakeParams {
        public bool recalculateNormals;
        public bool recalculateBounds;

        public MeshBakeParams (bool recalculateNormals, bool recalculateBounds) {
            this.recalculateNormals = recalculateNormals;
            this.recalculateBounds = recalculateBounds;
        }
    }
}