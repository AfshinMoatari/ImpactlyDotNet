using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public static class Rcads47
    {
        public static Survey Survey = new Survey()
        {
            ParentId = "TEMPLATE",
            Id = "rcads47",
            Name = "RCADS 47",
            LongName = "Revised Children's Anxiety and Depression Scale",
            Description = "Måler angst og depressionssymptomer (børneversion)",
            Max = 47 * 3,
            Min = 0,
            Thresholds = new List<int>() {47 * 3 / 2},
            IsNegative = true,
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
          new SurveyField()
          {
            Id = "rcads47q0",
            ParentId = "rcads47",
            Index = 0,
            Text = "Jeg bekymrer mig om ting",
          },
          new SurveyField()
          {
            Id = "rcads47q1",
            ParentId = "rcads47",
            Index = 1,
            Text = "Jeg føler mig ked af det eller tom indeni"
          },
          new SurveyField()
          {
            Id = "rcads47q2",
            ParentId = "rcads47",
            Index = 2,
            Text = "Når jeg har et problem, får jeg en underlig fornemmelse i min mave"
          },
          new SurveyField()
          {
            Id = "rcads47q3",
            ParentId = "rcads47",
            Index = 3,
            Text = "Jeg bekymrer mig, når jeg tror, jeg har klaret mig dårligt til noget"
          },
          new SurveyField()
          {
            Id = "rcads47q4",
            ParentId = "rcads47",
            Index = 4,
            Text = "Jeg ville være bange for at være alene derhjemme"
          },
          new SurveyField()
          {
            Id = "rcads47q5",
            ParentId = "rcads47",
            Index = 5,
            Text = "Der er ikke noget, som er rigtig sjovt længere"
          },
          new SurveyField()
          {
            Id = "rcads47q6",
            ParentId = "rcads47",
            Index = 6,
            Text = "Jeg føler mig bange, når jeg skal tage en prøve"
          },
          new SurveyField()
          {
            Id = "rcads47q7",
            ParentId = "rcads47",
            Index = 7,
            Text = "Jeg bliver bekymret, når jeg tror, nogen er sur på mig"
          },
          new SurveyField()
          {
            Id = "rcads47q8",
            ParentId = "rcads47",
            Index = 8,
            Text = "Jeg bekymrer mig om at være væk fra mine forældre"
          },
          new SurveyField()
          {
            Id = "rcads47q9",
            ParentId = "rcads47",
            Index = 9,
            Text = "Jeg bliver forstyrret af dumme eller skøre tanker eller billeder inde i mit hoved"
          },
          new SurveyField()
          {
            Id = "rcads47q10",
            ParentId = "rcads47",
            Index = 10,
            Text = "Jeg har svært ved at sove"
          },
          new SurveyField()
          {
            Id = "rcads47q11",
            ParentId = "rcads47",
            Index = 11,
            Text = "Jeg bekymrer mig om hvorvidt mit skolearbejde er dårligt"
          },
          new SurveyField()
          {
            Id = "rcads47q12",
            ParentId = "rcads47",
            Index = 12,
            Text = "Jeg er bekymret for, at noget forfærdeligt vil ske for nogen i min familie"
          },
          new SurveyField()
          {
            Id = "rcads47q13",
            ParentId = "rcads47",
            Index = 13,
            Text =
              "Jeg føler det lige pludseligt, som om jeg ikke kan trække vejret, når der ikke er nogen grund til det"
          },
          new SurveyField()
          {
            Id = "rcads47q14",
            ParentId = "rcads47",
            Index = 14,
            Text = "Jeg har problemer med min appetit"
          },
          new SurveyField()
          {
            Id = "rcads47q15",
            ParentId = "rcads47",
            Index = 15,
            Text =
              "Jeg er nødt til at blive ved med at tjekke, om jeg har gjort ting rigtigt (såsom at slukke for kontakten eller om døren er låst)"
          },
          new SurveyField()
          {
            Id = "rcads47q16",
            ParentId = "rcads47",
            Index = 16,
            Text = "Jeg føler mig bange, hvis jeg skal sove selv"
          },
          new SurveyField()
          {
            Id = "rcads47q17",
            ParentId = "rcads47",
            Index = 17,
            Text = "Jeg har svært ved at tage i skole om morgenen, fordi jeg er nervøs eller bange"
          },
          new SurveyField()
          {
            Id = "rcads47q18",
            ParentId = "rcads47",
            Index = 18,
            Text = "Jeg har ingen energi til ting"
          },
          new SurveyField()
          {
            Id = "rcads47q19",
            ParentId = "rcads47",
            Index = 19,
            Text = "Jeg bekymrer mig om, hvorvidt jeg ser dum ud"
          },
          new SurveyField()
          {
            Id = "rcads47q20",
            ParentId = "rcads47",
            Index = 20,
            Text = "Jeg er træt mange gange"
          },
          new SurveyField()
          {
            Id = "rcads47q21",
            ParentId = "rcads47",
            Index = 21,
            Text = "Jeg bekymrer mig om, hvorvidt dårlige ting vil ske for mig"
          },
          new SurveyField()
          {
            Id = "rcads47q22",
            ParentId = "rcads47",
            Index = 22,
            Text = "Jeg kan ikke få slemme eller skøre tanker ud af mit hoved"
          },
          new SurveyField()
          {
            Id = "rcads47q23",
            ParentId = "rcads47",
            Index = 23,
            Text = "Når jeg har et problem, banker mit hjerte rigtig hurtigt"
          },
          new SurveyField()
          {
            Id = "rcads47q24",
            ParentId = "rcads47",
            Index = 24,
            Text = "Jeg kan ikke tænke klart"
          },
          new SurveyField()
          {
            Id = "rcads47q25",
            ParentId = "rcads47",
            Index = 25,
            Text = "Jeg begynder lige pludselig at skælve eller ryste, når der ikke er nogen grund til det"
          },
          new SurveyField()
          {
            Id = "rcads47q26",
            ParentId = "rcads47",
            Index = 26,
            Text = "Jeg bekymrer mig om, hvorvidt noget slemt vil ske for mig"
          },
          new SurveyField()
          {
            Id = "rcads47q27",
            ParentId = "rcads47",
            Index = 27,
            Text = "Når jeg har et problem, føler jeg mig rystet"
          },
          new SurveyField()
          {
            Id = "rcads47q28",
            ParentId = "rcads47",
            Index = 28,
            Text = "Jeg føler mig værdiløs"
          },
          new SurveyField()
          {
            Id = "rcads47q29",
            ParentId = "rcads47",
            Index = 29,
            Text = "Jeg bekymrer mig om at lave fejl"
          },
          new SurveyField()
          {
            Id = "rcads47q30",
            ParentId = "rcads47",
            Index = 30,
            Text =
              "Jeg bliver nødt til at tænke specielle tanker (såsom numre eller ord) for at stoppe slemme ting fra at ske"
          },
          new SurveyField()
          {
            Id = "rcads47q31",
            ParentId = "rcads47",
            Index = 31,
            Text = "Jeg bekymrer mig om, hvad andre mennesker tænker om mig"
          },
          new SurveyField()
          {
            Id = "rcads47q32",
            ParentId = "rcads47",
            Index = 32,
            Text =
              "Jeg er bange for at være på trængte steder (såsom indkøbscentre, biografen, busser, fyldte legepladser)"
          },
          new SurveyField()
          {
            Id = "rcads47q33",
            ParentId = "rcads47",
            Index = 33,
            Text = "Lige pludselig føler jeg mig bange uden nogen årsag overhovedet…"
          },
          new SurveyField()
          {
            Id = "rcads47q34",
            ParentId = "rcads47",
            Index = 34,
            Text = "Jeg bekymrer mig om, hvad der kommer til at ske"
          },
          new SurveyField()
          {
            Id = "rcads47q35",
            ParentId = "rcads47",
            Index = 35,
            Text = "Jeg bliver lige pludselig svimmel eller føler jeg skal besvime, uden at der er nogen grund til det"
          },
          new SurveyField()
          {
            Id = "rcads47q36",
            ParentId = "rcads47",
            Index = 36,
            Text = "Jeg tænker på døden"
          },
          new SurveyField()
          {
            Id = "rcads47q37",
            ParentId = "rcads47",
            Index = 37,
            Text = "Jeg føler mig bange, hvis jeg skal snakke foran min klasse"
          },
          new SurveyField()
          {
            Id = "rcads47q38",
            ParentId = "rcads47",
            Index = 38,
            Text = "Mit hjerte begynder lige pludselig at slå for hurtigt uden nogen grund"
          },
          new SurveyField()
          {
            Id = "rcads47q39",
            ParentId = "rcads47",
            Index = 39,
            Text = "Jeg føler det, ligesom jeg ikke har lyst til at bevæge mig"
          },
          new SurveyField()
          {
            Id = "rcads47q40",
            ParentId = "rcads47",
            Index = 40,
            Text =
              "Jeg bekymrer mig om, at jeg lige pludselig får en bange følelse, når der ikke er noget at være bange for"
          },
          new SurveyField()
          {
            Id = "rcads47q41",
            ParentId = "rcads47",
            Index = 41,
            Text =
              "Jeg bliver nødt til at gøre nogle ting om og om igen (såsom at vaske mine hænder, rengøre eller putte ting i en bestemt orden)…"
          },
          new SurveyField()
          {
            Id = "rcads47q42",
            ParentId = "rcads47",
            Index = 42,
            Text = "Jeg føler mig bange for, at jeg vil gøre mig selv til grin foran andre mennesker…"
          },
          new SurveyField()
          {
            Id = "rcads47q43",
            ParentId = "rcads47",
            Index = 43,
            Text =
              "Jeg bliver nødt til at gøre nogle ting på lige præcis den rigtige måde, for at stoppe slemme ting fra at ske…"
          },
          new SurveyField()
          {
            Id = "rcads47q44",
            ParentId = "rcads47",
            Index = 44,
            Text = "Jeg bekymrer mig, når jeg går i seng om aftenen"
          },
          new SurveyField()
          {
            Id = "rcads47q45",
            ParentId = "rcads47",
            Index = 45,
            Text = "Jeg ville føle mig bange, hvis jeg skulle overnatte hjemmefra"
          },
          new SurveyField()
          {
            Id = "rcads47q46",
            ParentId = "rcads47",
            Index = 46,
            Text = "Jeg føler mig urolig"
          },
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>()
        {
          new FieldChoice()
          {
            Id = "rcads47o0",
            Index = 0,
            Text = "Slet ikke",
            Value = 0
          },
          new FieldChoice()
          {
            Id = "rcads47o1",
            Index = 1,
            Text = "Nogle gange",
            Value = 1
          },
          new FieldChoice()
          {
            Id = "rcads47o2",
            Index = 2,
            Text = "Ofte",
            Value = 2
          },
          new FieldChoice()
          {
            Id = "rcads47o3",
            Index = 3,
            Text = "Meget ofte",
            Value = 3
          },
        };
    }
}