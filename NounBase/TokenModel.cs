/*  https://github.com/hilodev/NounBase */
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NounBase
{
    public enum KeyTypesEnum
    {
        SingularProper = 1,
        PluralCollective = 2
    }
    public enum ValueTypesEnum { Null, ProperNounIdentityText, CommonNounCategoryText, Text, Numeric, Date, Boolean }
    /// <summary>
    /// Token for a word or acryonum as a singular proper noun or as a group collective plural noun.
    /// </summary>
    public class Token
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("Key")]
        public int KeyId { get; set; }
        public virtual Key Key { get; set; }
        [ForeignKey("Parent")]
        public int? ParentId { get; set; }
        public virtual Token Parent { get; set; }
        public virtual ICollection<Token> Children { get; set; }
        public virtual ICollection<KeyValue> KeyValues { get; set; }
    }
    /// <summary>
    /// KeyValue is an abstract noun of a <c>Noun</c>. 
    /// KeyValue is a labeled value of the attributed noun. 
    /// </summary>
    public class KeyValue// AbstractNoun
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("Attribute")]
        public int? KeyId { get; set; }
        public virtual Key Key { get; set; }
        public string Value { get; set; }
    }
    public class Key
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public KeyTypesEnum TokenType { get; set; }
        [Required]
        [MaxLength(16)]
        public string Label { get; set; }
        public string CommaDelimitedSynonyms { get; set; }
        public ValueTypesEnum ValueType { get; set; }
    }
}