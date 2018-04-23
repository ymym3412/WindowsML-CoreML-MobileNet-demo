using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage;
using Windows.AI.MachineLearning.Preview;

// mobilenet

namespace mobilenets
{
    public sealed class MobilenetModelInput
    {
        public VideoFrame data { get; set; }
    }

    public sealed class MobilenetModelOutput
    {
        public IList<string> classLabel { get; set; }
        public IDictionary<string, float> prob { get; set; }
        public MobilenetModelOutput()
        {
            this.classLabel = new List<string>();
            this.prob = new Dictionary<string, float>();
            for(int i=0; i<1000; i++)
            {
                this.prob.Add(i.ToString(), float.NaN);
            }
            
        }
    }

    public sealed class MobilenetModel
    {
        private LearningModelPreview learningModel;
        public static async Task<MobilenetModel> CreateMobilenetModel(StorageFile file)
        {
            LearningModelPreview learningModel = await LearningModelPreview.LoadModelFromStorageFileAsync(file);
            MobilenetModel model = new MobilenetModel();
            model.learningModel = learningModel;
            return model;
        }
        public async Task<MobilenetModelOutput> EvaluateAsync(MobilenetModelInput input) {
            MobilenetModelOutput output = new MobilenetModelOutput();
            LearningModelBindingPreview binding = new LearningModelBindingPreview(learningModel);
            binding.Bind("data", input.data);
            binding.Bind("classLabel", output.classLabel);
            binding.Bind("prob", output.prob);
            LearningModelEvaluationResultPreview evalResult = await learningModel.EvaluateAsync(binding, string.Empty);
            return output;
        }
    }
}
