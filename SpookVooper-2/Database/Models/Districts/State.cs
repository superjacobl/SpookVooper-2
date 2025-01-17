﻿using Shared.Models.Districts;
using SV2.Database.Models.Groups;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SV2.Database.Models.Districts;
public class State {
    [Column("id")]
    public long Id { get; set; }

    [Column("name", TypeName = "VARCHAR(64)")]
    public string? Name { get; set; }

    [Column("description", TypeName = "VARCHAR(512)")]
    public string? Description { get; set; }

    [Column("mapcolor")]
    public string MapColor { get; set; }

    [Column("groupid")]
    public long GroupId { get; set; }

    [NotMapped]
    public Group Group => DBCache.Get<Group>(GroupId)!;

    [Column("districtid")]
    public long DistrictId { get; set; }

    [NotMapped]
    [JsonIgnore]
    public District District => DBCache.Get<District>(DistrictId)!;

    [Column("governorid")]
    public long? GovernorId { get; set; }

    [NotMapped]
    public BaseEntity? Governor => BaseEntity.Find(GovernorId);

    /// <summary>
    /// In monthly rate
    /// </summary>
    public double? BasePropertyTax { get; set; }

    /// <summary>
    /// In monthly rate
    /// </summary>
    public double? PropertyTaxPerSize { get; set; }

    [NotMapped]
    public IEnumerable<Province> Provinces => DBCache.GetAll<Province>().Where(x => x.StateId == Id);

    [NotMapped]
    public long Population => Provinces.Sum(x => x.Population);

    public bool CanEdit(BaseEntity entity) {
        if (entity.Id == District.GovernorId) return true;
        if (Governor is not null) {
            if (Governor.EntityType == EntityType.User)
                return GovernorId == entity.Id;
            else {
                Group governorasgroup = (Group)Governor;

                return governorasgroup.HasPermission(entity, GroupPermissions.FullControl);
            }
        }
        return false;
    }

    public bool CanManageBuildingRequests(BaseEntity entity) {
        if (entity.Id == District.GovernorId) return true;
        if (Governor is not null) {
            if (Governor.EntityType == EntityType.User)
                return GovernorId == entity.Id;
            else {
                Group governorasgroup = (Group)Governor;
                return governorasgroup.HasPermission(entity, GroupPermissions.ManageBuildingRequests);
            }
        }
        return false;
    }
}