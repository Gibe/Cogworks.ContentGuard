﻿using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Cogworks.ContentGuard.Core.Services
{
    public interface IContentGuardService
    {
        string GetPageEditingUser(int pageId);
        bool IsLocked(int pageId, string ownerUsername);
        void Lock(int pageId, string ownerUsername);
        void Unlock(int pageId);
    }

    public class ContentGuardService : IContentGuardService
    {
        private readonly string _contentGuardRelationTypeAlias = "contentguard";

        private readonly IRelationService _relationService;
        private readonly IRelationType _contentGuardRelationType;
        public ContentGuardService(IRelationService relationService)
        {
            _relationService = relationService;
            _contentGuardRelationType = _relationService.GetRelationTypeByAlias(_contentGuardRelationTypeAlias);
        }

        public bool IsLocked(int pageId, string ownerUsername)
        {
            var existingLocks = _relationService.GetByParentOrChildId(pageId, _contentGuardRelationTypeAlias);

            return existingLocks.Any(x => !x.Comment.Equals(ownerUsername));
        }

        public void Lock(int pageId, string ownerUsername)
        {
            Unlock(pageId);

            var relation = new Relation(pageId, pageId, _contentGuardRelationType)
            {
                Comment = ownerUsername
            };

            _relationService.Save(relation);
        }

        public string GetPageEditingUser(int pageId)
        {
            var existingLocks = _relationService.GetByParentOrChildId(pageId, _contentGuardRelationTypeAlias);

            return existingLocks != null && existingLocks.Any()
                ? existingLocks.FirstOrDefault(x => x.ParentId.Equals(pageId)).Comment
                : string.Empty;
        }

        public void Unlock(int pageId)
        {
            var existingLocks =
                _relationService.GetByParentOrChildId(pageId, _contentGuardRelationTypeAlias);

            foreach (var pageLock in existingLocks)
            {
                _relationService.Delete(pageLock);
            }
        }
    }
}