package kyle.cdss;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class CTakesController {

    private static final Logger logger = LoggerFactory.getLogger(CTakesController.class);

    @GetMapping("/")
    public String index() {
        System.out.println("Logging hit (System.out)!");
        logger.info("Logging hit (logger)!");
        return "Java wrapper working for CDSS v4!";
    }

}
