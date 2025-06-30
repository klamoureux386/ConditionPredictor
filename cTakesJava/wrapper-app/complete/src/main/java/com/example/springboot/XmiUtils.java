package com.example.springboot;

import org.apache.uima.UIMAFramework;
import org.apache.uima.cas.CAS;
import org.apache.uima.fit.util.JCasUtil;
import org.apache.uima.jcas.JCas;
import org.apache.uima.json.JsonCasSerializer;
import org.apache.uima.resource.metadata.TypeSystemDescription;
import org.apache.uima.util.CasCreationUtils;
import org.apache.uima.util.CasIOUtils;
import org.apache.uima.util.XMLInputSource;

import org.apache.ctakes.typesystem.type.textsem.DiseaseDisorderMention;

import java.io.InputStream;
import java.io.StringWriter;
import java.nio.file.Files;
import java.nio.file.Paths;

public class XmiUtils {

    public static String XmiToJson(String xmiPath) throws Exception
    {
        //1. Set up TypeSystem
        String typeSystem = "C:\\Users\\Halifex\\Documents\\ConditionPredictor\\CTakesWrapper\\ctakes-7.0.0-compiled\\resources\\org\\apache\\ctakes\\typesystem\\types\\TypeSystem.xml";
        XMLInputSource tsdSrc = new XMLInputSource(typeSystem);
        TypeSystemDescription tsd = UIMAFramework.getXMLParser()
                .parseTypeSystemDescription(tsdSrc);
        tsd.resolveImports();

        // 2. Create a CAS with that type system
        CAS cas = CasCreationUtils.createCas(tsd, null, null);

        // 3. Load your XMI into the CAS in one line
        //String xmiPath = "C:\\ctakes-test\\output\\input_638860479074581492.txt.xmi";
        try (InputStream in = Files.newInputStream(Paths.get(xmiPath))) {
            CasIOUtils.load(in, cas);
        }

        // 4. Serialize the populated CAS to JSON
        JsonCasSerializer serializer = new JsonCasSerializer();
        serializer.setPrettyPrint(true);
        StringWriter sw = new StringWriter();
        serializer.serialize(cas, sw);
        System.out.println(sw.toString());

        return sw.toString();
    }
}
