using API.Models.Projects;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Linq;

namespace API.Extensions
{
    public interface ITagFiltersExtension
    {
        public bool CheckTags(List<ProjectTag> tagSetA, List<string> tagNameSetB);
    }

    public class TagFiltersExtension : ITagFiltersExtension
    {
        //input constrains is tagSetA
        public bool CheckTags(List<ProjectTag> tagSetA, List<string> tagNameSetB)
        {

            if (tagSetA.IsNullOrEmpty())
            {
                return true;
            }
            
            if (tagNameSetB.IsNullOrEmpty())
                return false;

            var tagNameSetA = tagSetA.Select(x => x.Name.ToLower()).ToList();
            var lowerCaseTagNameSetB = tagNameSetB.Select(tag => tag.ToLower()).ToList();
            //return CheckCondition(tagNameSetA, lowerCaseTagNameSetB);
            return IfSubset(tagNameSetA, lowerCaseTagNameSetB);
        }

        private bool CheckCondition(List<string> listA, List<string> listB)
        {
            return listA.Any(listB.Contains);
            //bool containsAllTags = listA.All(listB.Contains);
            //bool containsOneTag = listA.Count == 1 && listB.Contains(listA[0]);
            //return containsAllTags || containsOneTag;
        }

        //check if list b is a sub set of A:
        private bool IfSubset(List<string> listA, List<string> listB)
        {
            var setA = new HashSet<string>(listA);
            var setB = new HashSet<string>(listB);
            return setA.IsSubsetOf(setB);
        }
    }
}