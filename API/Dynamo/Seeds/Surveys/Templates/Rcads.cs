using System.Collections.Generic;
using API.Models.Strategy;

namespace API.Dynamo.Seeds.Surveys.Templates
{
    public static class Rcads
    {
        public static Survey Survey = new Survey()
        {
            ParentId = "TEMPLATE",
            Id = "rcads",
            Name = "RCADS",
            LongName = "Revised Children's Anxiety and Depression Scale",
            Description = "Måler angst og depressionssymptomer (forældreversion)",
            Max = 47 * 3,
            Min = 0,
            Thresholds = new List<int>() {47 * 3 / 2},
            IsNegative = true,
        };

        public static List<SurveyField> Fields = new List<SurveyField>()
        {
          new SurveyField()
          {
            Id = "rcadsq0",
            ParentId = "rcads",
            Index = 0,
            Text = "Mit barn bekymrer sig om ting."
          },
          new SurveyField()
          {
            Id = "rcadsq1",
            ParentId = "rcads",
            Index = 1,
            Text = "Mit barn føler sig ked af det og tom indeni…"
          },
          new SurveyField()
          {
            Id = "rcadsq2",
            ParentId = "rcads",
            Index = 2,
            Text = "Når mit barn har et problem får han/hun en underlig følelse i maven"
          },
          new SurveyField()
          {
            Id = "rcadsq3",
            ParentId = "rcads",
            Index = 3,
            Text = "Mit barn bekymrer sig når han/hun tror han/hun har klaret noget dårligt"
          },
          new SurveyField()
          {
            Id = "rcadsq4",
            ParentId = "rcads",
            Index = 4,
            Text = "Mit barn er bange for at være alene hjemme"
          },
          new SurveyField()
          {
            Id = "rcadsq5",
            ParentId = "rcads",
            Index = 5,
            Text = "Ingenting er rigtig sjovt for mit barn længere"
          },
          new SurveyField()
          {
            Id = "rcadsq6",
            ParentId = "rcads",
            Index = 6,
            Text = "Mit barn er bange for at tage en prøve i skolen"
          },
          new SurveyField()
          {
            Id = "rcadsq7",
            ParentId = "rcads",
            Index = 7,
            Text = "Mit barn bekymrer sig når han/hun tror nogen er vred på ham/hende"
          },
          new SurveyField()
          {
            Id = "rcadsq8",
            ParentId = "rcads",
            Index = 8,
            Text = "Mit barn bekymrer sig om at være væk fra mig"
          },
          new SurveyField()
          {
            Id = "rcadsq9",
            ParentId = "rcads",
            Index = 9,
            Text =
              "Mit barn er generet af slemme eller fjollede tanker eller billeder som foregår i hovedet på ham/hende"
          },
          new SurveyField()
          {
            Id = "rcadsq10",
            ParentId = "rcads",
            Index = 10,
            Text = "Mit barn har svært ved at sove"
          },
          new SurveyField()
          {
            Id = "rcadsq11",
            ParentId = "rcads",
            Index = 11,
            Text = "Mit barn bekymrer sig om at klare sit skolearbejde dårligt."
          },
          new SurveyField()
          {
            Id = "rcadsq12",
            ParentId = "rcads",
            Index = 12,
            Text = "Mit barn bekymrer sig om at noget frygteligt vil ske med en fra familien"
          },
          new SurveyField()
          {
            Id = "rcadsq13",
            ParentId = "rcads",
            Index = 13,
            Text =
              "Mit barn føler pludselig at han/hun ikke kan trække vejret selv når der ikke er nogen grund til dette"
          },
          new SurveyField()
          {
            Id = "rcadsq14",
            ParentId = "rcads",
            Index = 14,
            Text = "Mit barn har problemer med sin appetit"
          },
          new SurveyField()
          {
            Id = "rcadsq15",
            ParentId = "rcads",
            Index = 15,
            Text =
              "Mit barn er nødt til at blive ved med at checke at han/hun har gjort ting rigtigt (så som om kontakten er slået fra, eller døren er låst)"
          },
          new SurveyField()
          {
            Id = "rcadsq16",
            ParentId = "rcads",
            Index = 16,
            Text = "Mit barn er bange for at sove alene"
          },
          new SurveyField()
          {
            Id = "rcadsq17",
            ParentId = "rcads",
            Index = 17,
            Text = "Mit barn har svært ved at gå i skole om morgenen fordi han/hun føler sig nervøs eller bange"
          },
          new SurveyField()
          {
            Id = "rcadsq18",
            ParentId = "rcads",
            Index = 18,
            Text = "Mit barn har ingen energi til at lave ting"
          },
          new SurveyField()
          {
            Id = "rcadsq19",
            ParentId = "rcads",
            Index = 19,
            Text = "Mit barn bekymrer sig om at se fjollet ud"
          },
          new SurveyField()
          {
            Id = "rcadsq20",
            ParentId = "rcads",
            Index = 20,
            Text = "Mit barn er meget træt"
          },
          new SurveyField()
          {
            Id = "rcadsq21",
            ParentId = "rcads",
            Index = 21,
            Text = "Mit barn bekymrer sig om at væmmelige ting vil ske med ham/hende"
          },
          new SurveyField()
          {
            Id = "rcadsq22",
            ParentId = "rcads",
            Index = 22,
            Text = "Mit barn lader ikke til at kunne få væmmelige eller fjollede tanker ud af hovedet"
          },
          new SurveyField()
          {
            Id = "rcadsq23",
            ParentId = "rcads",
            Index = 23,
            Text = "Når mit barn har et problem, slår hans/hendes hjerte virkelig hurtigt"
          },
          new SurveyField()
          {
            Id = "rcadsq24",
            ParentId = "rcads",
            Index = 24,
            Text = "Mit barn kan ikke tænke klart"
          },
          new SurveyField()
          {
            Id = "rcadsq25",
            ParentId = "rcads",
            Index = 25,
            Text = "Mit barn begynder pludselig at dirre eller ryste, når der ikke er nogen grund til det"
          },
          new SurveyField()
          {
            Id = "rcadsq26",
            ParentId = "rcads",
            Index = 26,
            Text = "Mit barn bekymrer sig om at noget slemt vil ske med ham/hende"
          },
          new SurveyField()
          {
            Id = "rcadsq27",
            ParentId = "rcads",
            Index = 27,
            Text = "Når mit barn har et problem, føler han/hun sig rystet"
          },
          new SurveyField()
          {
            Id = "rcadsq28",
            ParentId = "rcads",
            Index = 28,
            Text = "Mit barn føler sig værdiløs"
          },
          new SurveyField()
          {
            Id = "rcadsq29",
            ParentId = "rcads",
            Index = 29,
            Text = "Mit barn bekymrer sig om at lave fejl"
          },
          new SurveyField()
          {
            Id = "rcadsq30",
            ParentId = "rcads",
            Index = 30,
            Text = "Mit barn skal tænke bestemte tanker (som tal eller ord) for at forhindre væmmelige ting i at ske"
          },
          new SurveyField()
          {
            Id = "rcadsq31",
            ParentId = "rcads",
            Index = 31,
            Text = "Mit barn bekymrer sig om hvad andre mennesker tænker om han/hende"
          },
          new SurveyField()
          {
            Id = "rcadsq32",
            ParentId = "rcads",
            Index = 32,
            Text =
              "Mit barn er bange for at være på trængte steder (som indkøbs centre, biografen, busser eller fyldte legepladser)"
          },
          new SurveyField()
          {
            Id = "rcadsq33",
            ParentId = "rcads",
            Index = 33,
            Text = "Lige pludselig bliver mit barn bange helt uden grund"
          },
          new SurveyField()
          {
            Id = "rcadsq34",
            ParentId = "rcads",
            Index = 34,
            Text = "Mit barn bekymrer sig om hvad der vil komme til at ske"
          },
          new SurveyField()
          {
            Id = "rcadsq35",
            ParentId = "rcads",
            Index = 35,
            Text = "Mit barn bliver pludselig ør og svimmel når der ikke er nogen grund til det"
          },
          new SurveyField()
          {
            Id = "rcadsq36",
            ParentId = "rcads",
            Index = 36,
            Text = "Mit barn tænker på døden"
          },
          new SurveyField()
          {
            Id = "rcadsq37",
            ParentId = "rcads",
            Index = 37,
            Text = "Mit barn er bange når han/hun skal tale foran klassen"
          },
          new SurveyField()
          {
            Id = "rcadsq38",
            ParentId = "rcads",
            Index = 38,
            Text = "Mit barns hjerte begynder pludselig at slå for hurtigt uden grund"
          },
          new SurveyField()
          {
            Id = "rcadsq39",
            ParentId = "rcads",
            Index = 39,
            Text = "Mit barn føler, at han/hun ikke vil bevæge sig"
          },
          new SurveyField()
          {
            Id = "rcadsq40",
            ParentId = "rcads",
            Index = 40,
            Text =
              "Mit barn bekymrer sig om at han/hun pludselig vil føle sig bange når der ikke er noget at være bange for"
          },
          new SurveyField()
          {
            Id = "rcadsq41",
            ParentId = "rcads",
            Index = 41,
            Text =
              "Mit barn er nødt til at gøre bestemte ting om og om igen (som vaske hænder, rense, sætte ting i en bestemt orden)"
          },
          new SurveyField()
          {
            Id = "rcadsq42",
            ParentId = "rcads",
            Index = 42,
            Text = "Mit barn føler sig bange for at han/hun vil virke fjollet foran andre mennesker"
          },
          new SurveyField()
          {
            Id = "rcadsq43",
            ParentId = "rcads",
            Index = 43,
            Text = "Mit barn skal gøre bestemte ting på den helt rigtige måde for at forhindre slemme ting i at ske"
          },
          new SurveyField()
          {
            Id = "rcadsq44",
            ParentId = "rcads",
            Index = 44,
            Text = "Mit barn bekymrer sig når han/hun ligger i sengen om aftenen"
          },
          new SurveyField()
          {
            Id = "rcadsq45",
            ParentId = "rcads",
            Index = 45,
            Text = "Mit barn ville blive bange hvis han/hun skulle væk hjemmefra natten ove"
          },
          new SurveyField()
          {
            Id = "rcadsq46",
            ParentId = "rcads",
            Index = 46,
            Text = "Mit barn føler sig rastløs"
          },
        };

        public static List<FieldChoice> Choices = new List<FieldChoice>()
        {
          new FieldChoice()
          {
            Id = "rcadso0",
            Index = 0,
            Text = "Slet ikke",
            Value = 0
          },
          new FieldChoice()
          {
            Id = "rcadso1",
            Index = 1,
            Text = "Nogle gange",
            Value = 1
          },
          new FieldChoice()
          {
            Id = "rcadso2",
            Index = 2,
            Text = "Ofte",
            Value = 2
          },
          new FieldChoice()
          {
            Id = "rcadso3",
            Index = 3,
            Text = "Meget ofte",
            Value = 3
          },
        };
    }
}